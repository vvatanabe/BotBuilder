﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Form;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Bot.Sample.PizzaBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private static IForm<PizzaOrder> MakePizzaForm()
        {
            var builder = FormModelBuilder<PizzaOrder>.Start();

            const bool NoNumbers = false;
            if (NoNumbers)
            {
                builder.Configuration.DefaultPrompt.ChoiceFormat = "{1}";
            }
            else
            {
                builder.Configuration.DefaultPrompt.ChoiceFormat = "{0}. {1}";
            }

            ConditionalDelegate<PizzaOrder> isBYO = (pizza) => pizza.Kind == PizzaOptions.BYOPizza;
            ConditionalDelegate<PizzaOrder> isSignature = (pizza) => pizza.Kind == PizzaOptions.SignaturePizza;
            ConditionalDelegate<PizzaOrder> isGourmet = (pizza) => pizza.Kind == PizzaOptions.GourmetDelitePizza;
            ConditionalDelegate<PizzaOrder> isStuffed = (pizza) => pizza.Kind == PizzaOptions.StuffedPizza;

            var model = builder
                // .Field(nameof(PizzaOrder.Choice))
                .Field(nameof(PizzaOrder.Size))
                .Field(nameof(PizzaOrder.Kind))
                .Field("BYO.Crust", isBYO)
                .Field("BYO.Sauce", isBYO)
                .Field("BYO.Toppings", isBYO)
                .Field(nameof(PizzaOrder.GourmetDelite), isGourmet)
                .Field(nameof(PizzaOrder.Signature), isSignature)
                .Field(nameof(PizzaOrder.Stuffed), isStuffed)
                .AddRemainingFields()
                .Confirm("Would you like a {Size}, {BYO.Crust} crust, {BYO.Sauce}, {BYO.Toppings} pizza?", isBYO)
                .Confirm("Would you like a {Size}, {&Signature} {Signature} pizza?", isSignature, dependencies: new string[] { "Size", "Kind", "Signature" })
                .Confirm("Would you like a {Size}, {&GourmetDelite} {GourmetDelite} pizza?", isGourmet)
                .Confirm("Would you like a {Size}, {&Stuffed} {Stuffed} pizza?", isStuffed)
                .Build()
                ;

            IForm<PizzaOrder> form = new Form<PizzaOrder>("PizzaForm", model);
            return form;
        }

        public static IDialogNew MakeRoot()
        {
            return new PizzaOrderDialog(MakePizzaForm);
        }

        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and reply to it
        /// </summary>
        [ResponseType(typeof(Message))]
        public async Task<HttpResponseMessage> Post([FromBody]Message message)
        {
            return await CompositionRoot.PostAsync(this.Request, message, MakeRoot);
        }
    }
}