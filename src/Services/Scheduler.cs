using Google.Apis.Auth.OAuth2;
using Google.Apis.Docs.v1;
using Google.Apis.Util;
using Microsoft.EntityFrameworkCore;
using System.Timers;
using Telegram.Bot.Examples.WebHook.Data;
using Telegram.Bot.Examples.WebHook.Google;
using Google.Apis.Services;
using Telegram.Bot.Models;

namespace Telegram.Bot.Examples.WebHook.Services
{
    public class Scheduler
    {
        private readonly IServiceProvider serviceProvider;
        private System.Timers.Timer aTimer;
        private ApplicationDbConext context;
        private GoogleFlow flow;
        public bool processing = false;

        public Scheduler(IServiceProvider sp)
        {
            serviceProvider = sp;
            context = sp.GetRequiredService<ApplicationDbConext>();
            flow = sp.GetRequiredService<GoogleFlow>();
        }
        

        public async Task Start()
        {
            await Task.Delay(1);
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent!;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (processing)
            {
                return;
            }
            processing = !processing;

            //await Task.Run(async () =>
            //{
            //    var users = await context.Users.ToListAsync();
            //    foreach (var user in users)
            //    {
            //       var token = await flow.flow.LoadTokenAsync(user.Id.ToString(), CancellationToken.None);
            //        if (token == null || token.IsExpired(SystemClock.Default))
            //            continue;
            //        var panel = await context.Panels.Where(panel => panel.Id == user.Id).FirstAsync();
            //        if (panel == null)
            //            continue;
            //        var docs = await context.Docs.Where(doc => doc.Owner.Id == user.Id).ToListAsync();
            //        UserCredential credential = new(flow.flow, user.Id.ToString(), token);
            //        var docService = new DocsService(new BaseClientService.Initializer()
            //        {
            //            HttpClientInitializer = credential
            //        });

            //        foreach (var item in docs)
            //        {
            //            var text = await panel.DownloadDocument(item.Do)
            //        }
            //        {

            //        }
            //    }
            //    await Task.Delay(5000);
            //    processing = !processing;
            //});
            await Task.Delay(10000);
            processing = !processing;
        }

        public void Dispose()
        {
            if(aTimer != null)
            {
                aTimer.Dispose();
            }
        }
        
    }

}
