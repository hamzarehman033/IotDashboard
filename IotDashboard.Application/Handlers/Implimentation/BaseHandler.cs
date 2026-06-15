using AutoMapper;
using IotDashboard.Application.Dtos;
using IotDashboard.Application.Handlers.Interface;
using IotDashboard.Application.Util;
using IotDashboard.Domain.Entities;
using IotDashboard.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using AutoMapper.Internal;
using IotDashboard.Application.Validators;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Reflection;



namespace IotDashboard.Application.Handlers.Implimentation
{
    public class BaseHandler<TVM, TModel> : IBaseHandler<TVM> where TVM : class where TModel : BaseEntity
    {
        protected readonly IBaseRepository<TModel> _repo;
        protected readonly IMapper _mapper;
        protected readonly IValidator<TVM> _validator;
        protected readonly FilterValidator<TVM> _filterValidator;
        protected readonly string _success;
        protected readonly string _error;
        protected IHttpContextAccessor _httpContextAccessor;
        private readonly PropertyInfo? _customerIdProperty;

        public BaseHandler(IBaseRepository<TModel> repo, IMapper mapper, 
            IValidator<TVM> validator, FilterValidator<TVM> filterValidator,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _mapper = mapper;
            _validator = validator;
            _filterValidator = filterValidator;
            _success = httpContextAccessor.GetResourceString("global.status.success");
            _error = httpContextAccessor.GetResourceString("global.status.error");
            _httpContextAccessor = httpContextAccessor;
            _customerIdProperty = typeof(TModel).GetProperty("CustomerId");
        }

        public virtual async Task<Response<TVM>> CreateAsync(TVM model)
        {
            Response<TVM> response = new Response<TVM>();
            ValidationResult result = await _validator.ValidateAsync(model);
            if (result.IsValid)
            {
                TModel obj = _mapper.Map<TModel>(model);
                obj.IsActive = true;
                if (_customerIdProperty != null)
                {
                    if (!GetCustomerId(out var customerId))
                        return ErrorResponse<TVM>("A valid X-Customer-Id header is required.");
                    _customerIdProperty.SetValue(obj, customerId);
                }
                await _repo.CreateAsync(obj);
                response.Data = model;
                response.Status = _success;
            }
            else
            {
                response.Status = _error;
                response.Message = result.ToErrorMessage();
            }
            return response;
        }

        public virtual async Task<Response<TVM>> DeleteAsync(long Id)
        {
            if (_customerIdProperty != null)
            {
                if (!GetCustomerId(out var customerId))
                    return ErrorResponse<TVM>("A valid X-Customer-Id header is required.");
                var model = await GetTenantRecord(Id, customerId);
                if (model == null)
                    return ErrorResponse<TVM>("Record not found.");
                model.IsActive = false;
                await _repo.UpdateAsync(model);
            }
            else
            {
                await _repo.DeleteAsync(Id);
            }
            Response<TVM> response = new Response<TVM>();
            response.Status = _success;
            response.Message.Add(_httpContextAccessor.GetResourceString("global.delete"));
            return response;
        }

        public virtual async Task<Response<PagerModel<TVM>>> GetAllAsync(int pageSize = 10, int currentPage = 1, IEnumerable<FilterVM> filters = null)
        {
            Response<PagerModel<TVM>> response = new Response<PagerModel<TVM>>() { Status = _error };
            IQueryable<TModel> queryable = _repo.GetAllAsync();
            if (_customerIdProperty != null)
            {
                if (!GetCustomerId(out var customerId))
                {
                    response.Message.Add("A valid X-Customer-Id header is required.");
                    return response;
                }
                queryable = queryable.Where(x => (long)_customerIdProperty.GetValue(x)! == customerId);
            }
            if (filters != null && filters.Count() > 0)
            {
                var validationResult = await _filterValidator.ValidateAsync(filters);
                if (!validationResult.IsValid)
                {
                    response.Message = validationResult.ToErrorMessage();
                    return response;
                }
                queryable = queryable.Where(GetFitlerPredicate(filters));
            }
            response.Data = await queryable.ToPageAsync<TVM, TModel>(pageSize, currentPage, _mapper);
            response.Status = _success;
            return response;
        }


        public virtual async Task<Response<TVM>> GetByIdAsync(long Id)
        {
            Response<TVM> response = new Response<TVM>();
            TModel? model;
            if (_customerIdProperty != null)
            {
                if (!GetCustomerId(out var customerId))
                {
                    response.Status = _error;
                    response.Message.Add("A valid X-Customer-Id header is required.");
                    return response;
                }
                model = await GetTenantRecord(Id, customerId);
            }
            else
            {
                model = await _repo.GetByIdAsync(Id);
            }
            if (model == null)
            {
                response.Status = _error;
                response.Message.Add("Record not found.");
                return response;
            }
            response.Status = _success;
            response.Data = _mapper.Map<TVM>(model);
            return response;
        }

        public virtual async Task<Response<TVM>> UpdateAsync(long Id, TVM model)
        {
            Response<TVM> response = new Response<TVM>();
            ValidationResult result = await _validator.ValidateAsync(model);
            if (result.IsValid)
            {
                TModel obj = _mapper.Map<TModel>(model);
                obj.Id = Id;
                if (_customerIdProperty != null)
                {
                    if (!GetCustomerId(out var customerId))
                        return ErrorResponse<TVM>("A valid X-Customer-Id header is required.");
                    _customerIdProperty.SetValue(obj, customerId);
                }
                await _repo.UpdateAsync(obj);
                response.Data = model;
                response.Status = _success;
            }
            else
            {
                response.Status = _error;
                response.Message = result.ToErrorMessage();
            }
            return response;
        }

        private string GetDBModelPropertyByVMProperty<TSrc, TDest>(string vmProperty, IConfigurationProvider configuration)
        {
            var internalAPI = InternalApi.Internal(configuration);
            var map = internalAPI.FindTypeMapFor<TSrc, TDest>();
            var destProp = map.PropertyMaps.FirstOrDefault(x => x.SourceMember?.Name.ToLower().Equals(vmProperty.ToLower()) == true)?
                                .DestinationMember?.Name;
            return destProp;
        }

        private Expression<Func<TModel, bool>> GetFitlerPredicate(IEnumerable<FilterVM> filters)
        {
            var parameter = Expression.Parameter(typeof(TModel), "x");
            Expression predicate = Expression.New(typeof(TModel));
            string? oldOperator = string.Empty;
            foreach (var filter in filters)
            {
                var property = Expression.Property(parameter, GetDBModelPropertyByVMProperty<TVM, TModel>(filter.Key, _mapper.ConfigurationProvider));
                var constant = Expression.Constant(Convert.ChangeType(filter.Value, property.Type));
                var binaryExpression = GetBinaryExpression(property, constant, filter.Operator);
                var filterPredicate = Expression.Lambda<Func<TModel, bool>>(binaryExpression, parameter).Body;
                if (!string.IsNullOrEmpty(oldOperator))
                    predicate = CombinePredicates(predicate, filterPredicate, oldOperator);
                else
                    predicate = filterPredicate;
                oldOperator = filter.PostOperator;
            }

            return Expression.Lambda<Func<TModel, bool>>(predicate, parameter);
        }

        private Expression CombinePredicates(Expression left, Expression right, string postOperator)
        {
            switch (postOperator.ToLower())
            {
                case "and":
                    return Expression.AndAlso(left, right);
                case "or":
                    return Expression.OrElse(left, right);
                default:
                    throw new NotSupportedException(string.Format(_httpContextAccessor.GetResourceString("validations.filter.postOperator"), postOperator));
            }
        }

        private Expression GetBinaryExpression(Expression left, Expression right, string @operator)
        {
            switch (@operator.ToLower())
            {
                case "equals":
                    return Expression.Equal(left, right);
                case "contains":
                    return Expression.Call(left, typeof(string).GetMethod("Contains", new[] { typeof(string) }), right);
                case "lessthan":
                    return Expression.LessThan(left, right);
                case "greaterthan":
                    return Expression.GreaterThan(left, right);
                case "lessthanorequal":
                    return Expression.LessThanOrEqual(left, right);
                case "greaterthanorequal":
                    return Expression.GreaterThanOrEqual(left, right);
                case "startswith":
                    return Expression.Call(left, typeof(string).GetMethod("StartsWith", new[] { typeof(string) }), right);
                case "endswith":
                   return Expression.Call(left, typeof(string).GetMethod("EndsWith", new[] { typeof(string) }), right);
                // Add more cases for other operators as needed
                default:
                    throw new NotSupportedException(string.Format(_httpContextAccessor.GetResourceString("validations.filter.operator"), @operator));
            }
        }

        private bool GetCustomerId(out long customerId)
        {
            customerId = 0;
            var headerValue = _httpContextAccessor.HttpContext?.Request?.Headers["X-Customer-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(headerValue) || !long.TryParse(headerValue, out customerId) || customerId <= 0)
                return false;
            return true;
        }

        private async Task<TModel?> GetTenantRecord(long id, long customerId)
        {
            return await _repo.GetAllAsync()
                .Where(x => (long)_customerIdProperty!.GetValue(x)! == customerId && x.Id == id)
                .FirstOrDefaultAsync();
        }

        private Response<TData> ErrorResponse<TData>(string message)
        {
            return new Response<TData>
            {
                Status = _error,
                Message = new List<string> { message }
            };
        }
    }
}
