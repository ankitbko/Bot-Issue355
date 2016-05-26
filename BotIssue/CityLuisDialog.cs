using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotIssue
{
    [Serializable]
    [LuisModel("c413b2ef-382c-45bd-8ff0-f76d60e2a821", "6d0966209c6e4f6b835ce34492f3e6d9")]
    public class CityLuisDialog : LuisDialog<object>
    {
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("I'm sorry. I didn't understand you.");
            context.Done(default(object));
        }

        [LuisIntent("builtin.intent.places.find_place")]
        public async Task Location(IDialogContext context, LuisResult result)
        {
            var entities = new List<EntityRecommendation>(result.Entities);
            EntityRecommendation loc;

            //Comment the full If Block
            if (result.TryFindEntity("builtin.places.absolute_location", out loc))
                entities.Add(new EntityRecommendation(type: nameof(CityForm.Location)) { Entity = loc.Entity });
            //

            var form = new FormDialog<CityForm>(new CityForm(), CityForm.BuildForm, FormOptions.PromptInStart, entities);
            context.Call(form, BuildFormComplete);
        }

        private async Task BuildFormComplete(IDialogContext context, IAwaitable<CityForm> result)
        {
            try
            {
                var emp = await result;

                await context.PostAsync("Done");

                context.Wait(MessageReceived);
            }
            catch (FormCanceledException<CityForm> e)
            {
                string reply;
                if (e.InnerException == null)
                {
                    reply = $"You quit --maybe you can finish next time!";
                }
                else
                {
                    reply = "Sorry, I've had a short circuit.  Please try again.";
                }
                await context.PostAsync(reply);
            }
        }
    }

    [Serializable]
    public class CityForm
    {
        public string Location { get; set; }

        public static IForm<CityForm> BuildForm()
        {
            string Airports = "Aberdeen; Aberdeen Airport Belfast; Belfast International Airport Aldergrove Belfast; George Best Belfast City Airport Birmingham; Birmingham International Airport Blackpool; Blackpool Airport Bristol; Bristol Airport Cardiff; Cardiff International Airport Derby; East Midlands Apt Doncaster; Robin Hood Airport Durham; Durham Tees Valley Airport Edinburgh; Edinburgh Airport Exeter; Exeter Airport Glasgow; Glasgow International Airport Gloucester; Staverton Airport Humberside; Humberside Airport Inverness; Inverness Airport Jersey; Jersey Airport (Meet&Greet) Leeds Bradford Airport; (Meet & Greet) Liverpool; Liverpool Airport London; Gatwick Airport North Terminal London; Gatwick Airport South Terminal London; London Heathrow Airport, T1,T2,T3,T4 London; London Heathrow Airport, T5 London; London City Airport London; London Luton Airport London; London Stansted Airport Manchester; Manchester Airport Newcastle; Newcastle Airport Newquay; Newquay Airport Norwich; Norwich Airport Prestwick; Prestwick Airport Southampton; Southampton International Airport";

            return new FormBuilder<CityForm>()
                .Field(new FieldReflector<CityForm>(nameof(Location))
                    .SetType(null)
                    .SetDefine(async (state, field) =>
                    {
                        foreach (var airport in Airports.Split(';'))
                        {
                            field.AddDescription(airport, Language.CamelCase(airport));
                            field.AddTerms(airport, Language.GenerateTerms(Language.CamelCase(airport), 3));  // change to 5
                        }
                        return true;
                    })
                    .SetPrompt(new PromptAttribute("Select a location"))
                )
                .AddRemainingFields()
                .Build();
        }
    }
}
