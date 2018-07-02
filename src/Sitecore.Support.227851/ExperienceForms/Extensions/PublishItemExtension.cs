using System;
using System.Linq;
using System.Text.RegularExpressions;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Publishing;
using Sitecore.ExperienceForms.Mvc.Constants;
using Sitecore.Data.Fields;

namespace Sitecore.Support.ExperienceForms.Extensions
{
    public class PublishItemExtension
    {
        private const string GuidPattern = @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}";
        private const string experienceFormsMvcId = "6B5B0FE6-5C85-4487-BA7E-F8C2ECC186A8";
        readonly string targetDatabaseName;

        public PublishItemExtension(string targetDatabaseName)
        {
            this.targetDatabaseName = targetDatabaseName;
        }

        public void PublishFormChildItems(object sender, EventArgs args)
        {
            var sitecoreArgs = args as Events.SitecoreEventArgs;
            var parameters = sitecoreArgs?.Parameters.FirstOrDefault() as Publisher;

            if (parameters != null && parameters.Options.PublishRelatedItems && parameters.Options.Mode == PublishMode.SingleItem)
            {
                var targetItem = parameters.Options.RootItem;

                var finalRenderings = LayoutField.GetFieldValue(targetItem.Fields[FieldIDs.FinalLayoutField]);

                if (finalRenderings != null && finalRenderings.Contains(experienceFormsMvcId))
                {
                    var sourceDatabase = parameters.Options.SourceDatabase;

                    var renderingsIDs = Regex.Matches(finalRenderings, GuidPattern);

                    foreach (var id in renderingsIDs)
                    {
                        var item = sourceDatabase.GetItem(id.ToString());
                        if (item != null)
                        {
                            if (item.TemplateID == TemplateIds.FormTemplateId)
                            {
                                var webDatabase = Database.GetDatabase(targetDatabaseName);
                                this.PublishItem(item, sourceDatabase, webDatabase, PublishMode.SingleItem);
                            }
                        }
                    }
                }
            }
        }

        private void PublishItem(Item item, Database sourceDB, Database targetDB, PublishMode mode)
        {
            var publishOptions = new PublishOptions(sourceDB, targetDB, mode, item.Language, DateTime.Now)
            {
                RootItem = item,
                Deep = true
            };

            var publisher = new Publisher(publishOptions);

            publisher.Publish();
        }
    }
}
