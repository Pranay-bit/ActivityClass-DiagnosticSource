using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ActivityClass_DiagnosticSource
{
  public class ActivityClass
    {
        private static readonly DiagnosticSource diagnosticSource
            = new DiagnosticListener(typeof(ActivityClass).FullName);

        public int GetRandomNumber()
        {
            if (diagnosticSource.IsEnabled(typeof(ActivityClass).FullName))
            {
                diagnosticSource.Write($"{typeof(ActivityClass).FullName}.StartGenerateRandom", null);
            }
            var number = new Random().Next();

            if (diagnosticSource.IsEnabled(typeof(ActivityClass).FullName))
            {
                diagnosticSource.Write($"{typeof(ActivityClass).FullName}.EndGenerateRandom", new {RandomNumber = number });
            }
            return number;
        }

            //        Activity

            // var activity = new Activity("MyActivity");

            // _diagnostics.StartActivity(activity, new { Property1 = prop1, Property2 = prop2});

            // Do work - normal application/Library code

            // activity.AddBaggage("MyBaggageId", value);
            // activity.AddTag("MyTagId", value)

            // _logger.Log(activity.Id);
            // _logger.Log(activity.ParentId)

            // _diagnostics.StopActivity(activity, id);

public static async Task DoThingAsync(int id)
        {
            var activity = new Activity(nameof(DoThingAsync));

            if(diagnosticSource.IsEnabled(typeof(ActivityClass).FullName))
            {
                diagnosticSource.StartActivity(activity, new { IdArg = id });
            }

            activity.AddTag("MyTagId", "valueTags");
            activity.AddBaggage("MyBaggageId", "valueInBaggage");


            var httpClient = new HttpClient();
            await httpClient.GetAsync("http://localhost:5000/values");

            if (diagnosticSource.IsEnabled(typeof(ActivityClass).FullName))
            {
                diagnosticSource.StopActivity(activity, new { IdArg = id });
            }
        }
    }
}
