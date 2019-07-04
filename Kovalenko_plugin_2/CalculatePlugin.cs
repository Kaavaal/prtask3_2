using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Kovalenko_plugin_2
{
    public class CalculatePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // global settings
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Post Image
            Entity data = context.PostEntityImages["PostImage"];

            // Get <Amount> field
            var amount = data.GetAttributeValue<Money>("cr486_amount");

            // Get <lookup> field
            var parent = data.GetAttributeValue<EntityReference>("cr486_parent");

            // Query to get <child Amount field>
            QueryExpression query = new QueryExpression
            {
                EntityName = "cr486_kovalenko_2019_child",
                ColumnSet = new ColumnSet(true),
                Criteria = new FilterExpression
                {
                    Conditions =
                        {
                            new ConditionExpression
                            {
                                AttributeName = "cr486_parent",
                                Operator = ConditionOperator.Equal,
                                Values = { parent.Id }
                            }
                        }
                }
            };
            DataCollection<Entity> ChildAmountEntity = service.RetrieveMultiple(query).Entities;

            // Check child's amount
            if (ChildAmountEntity != null)
            {
                decimal sum = 0;

                foreach (Entity count in ChildAmountEntity)
                {
                    sum = sum + ((Money)count.Attributes["cr486_amount"]).Value;
                }

                // Parent amount entity
                var ParentAmountEntity = service.Retrieve("cr486_kovalenko_2019", parent.Id, new ColumnSet(true));
                if (ParentAmountEntity != null)
                {
                    // set <parent amount value>
                    ParentAmountEntity["cr486_amount"] = new Money(sum);
                    service.Update(ParentAmountEntity);
                }
            }
        }
    }
}
