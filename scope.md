| Provider | Typical cost (this chatbot) | Main limits |
|---|---|---|
| **Groq** (Llama 8B) | **$0** on free tier; paid is very cheap (~$0.05–0.08 / 1M tokens) | ~30 requests/min; 8B has a **high** daily cap; 70B often ~**1,000/day**. No enterprise SLA on free. Data **leaves Azure**. |
| **Groq** (Llama 70B) | Same (free until cap) | Same RPM; **much lower** daily cap. Better answers/tools, easier to hit the wall. |
| **Google Gemini** (Flash) | **$0** on AI Studio free tier; paid Flash is still cheap | Free: tens of req/min and ~**1,500/day** (varies by model). **Free-tier prompts may be used to improve Google products.** Extra vendor. |
| **Azure OpenAI / Foundry** (mini / nano) | **No free API.** Pay-as-you-go: often **~$5–30/month** at a few hundred chats/day | Needs **Azure quota** and a deployment. Mini/nano are enough; full GPT-5 is wasted spend. Data can stay **in Azure**. |
| **OpenAI API** (same mini class) | Same token prices as Azure (~**$0.15 / $0.60** per 1M in/out for 4o-mini-class; nano/mini GPT-5 similar order) | No Azure residency. Fast to get a key. **Not** ChatGPT Plus. |
| **Static FAQ** (no model) | **$0** | Only canned/similar questions. No live data. No rate limit from a vendor. |

**For this feature:** Groq = cheapest to try; Gemini = similar free trial with stricter data terms; Azure = what you pay for production/compliance. A few hundred short questions/day is **cents to tens of dollars**, not hundreds, on any paid mini/Flash/Groq plan.