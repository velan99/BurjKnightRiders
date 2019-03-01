using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Links;
using Sitecore.Resources;
using Sitecore.Shell;
using Sitecore.Shell.Applications.ContentEditor.Tabs;
using Sitecore.Shell.DeviceSimulation;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web.UI;
using Sitecore.Web.UI.Framework.Scripts;
using Sitecore.Web.UI.Sheer;
using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

namespace Sitecore.Scientist.Analytics.Commands
{
    public class SEOCheck : Command
    {
        public SEOCheck()
        {
        }

        /// <summary>
        /// Executes the command in the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            if ((int)context.Items.Length != 1)
            {
                return;
            }
            DeviceSimulationUtil.DeactivateSimulators();
            Item items = context.Items[0];
            ShortID shortID = items.ID.ToShortID();
            string str = string.Concat("SEO Check", shortID);
            if ((new EditorTabsClientManager()).ShowEditorTabs.Any<ShowEditorTab>((ShowEditorTab tab) =>
            {
                if (tab.Command != "contenteditor:seocheck")
                {
                    return false;
                }
                return tab.Id == str;
            }))
            {
                SheerResponse.Eval(string.Concat("scContent.onEditorTabClick(null, null, '", str, "')"));
                return;
            }
            var url = LinkManager.GetItemUrl(items, new UrlOptions() { AlwaysIncludeServerUrl = true });
            Uri myUri = new Uri(url);
            UrlString urlString = new UrlString(myUri.Host + "/sitecore/shell/-/xaml/Sitecore.Shell.Applications.ContentEditor.Editors.Preview.aspx");
            items.Uri.AddToUrlString(urlString);
            UIUtil.AddContentDatabaseParameter(urlString);
            WebClient client = new WebClient();
            String htmlCode = client.DownloadString(urlString.ToString());
            //Below code is to open the SEO Report
            //SheerResponse.Eval((new ShowEditorTab()
            //{
            //    Command = "contenteditor:seocheck",
            //    Header = Translate.Text("SEO Check"),
            //    Title = string.Concat(Translate.Text("SEO Check"), ": ", (!UserOptions.View.UseDisplayName ? items.Name : items.DisplayName)),
            //    Icon = Images.GetThemedImageSource("Network/16x16/environment.png"),
            //    Url = urlString.ToString(),
            //    Id = str,
            //    Closeable = true,
            //    Activate = true,
            //    Language = items.Language,
            //    Version = items.Version
            //}).ToString());
        }

        /// <summary>
        /// Queries the state of the command.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The state of the command.</returns>
        public override CommandState QueryState(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");
            if (!Settings.Preview.Enabled)
            {
                return CommandState.Hidden;
            }
            if ((int)context.Items.Length != 1)
            {
                return CommandState.Disabled;
            }
            if (!base.HasField(context.Items[0], FieldIDs.LayoutField))
            {
                return CommandState.Hidden;
            }
            return base.QueryState(context);
        }
    }
}