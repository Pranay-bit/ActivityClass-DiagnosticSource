using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace ActivityClass_DiagnosticSource
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Subscribe();
            
            var number = new ActivityClass().GetRandomNumber();

            var httpClient = new HttpClient();
            await httpClient.GetAsync("https://www.nttdata.com");

            Console.WriteLine("Hello World!");
        }
        private static void Subscribe()
        {
            DiagnosticListener.AllListeners.Subscribe(new Subscriber());
        }
    }
    class MyLibraryListener : IObserver<KeyValuePair<string, object>>
        {
        public void OnCompleted()
        {}
        public void OnError(Exception error)
        {}

                        //                                  Activity Current

                        //Points to the currently active activity which flows with async call and is available during request processing.

                        //DiagnosticSource.StartActivity(myActivity): sets myActivity to Activity.Current

                        //DiagnosticSource.StopActivity(myActivity): clears Activity.Current

                        //class MyLibraryListener : IObserve<KeyValuePair<string, object>>
                        //        {
                        //            public void OnNext(KeyValuePair<string, object> receivedEvent)
                        //            {
                        //                var currentActivity = Activity.Current;

                        //                var id = currentActivity.Id;
                        //                var parentId = currentActivity.ParentId;
                        //            }
                        //        }


        public void OnNext(KeyValuePair<string, object> keyValue)
        {
            switch (keyValue.Key)
            {
                case "DoThingAsync.Start":
                    Console.WriteLine($"DoThingAsync.Start - activity id:{Activity.Current?.Id}");
                   break;
                case "DoThingAsync.Stop":
                    Console.WriteLine("DoThingAsync.Stop");

                    if(Activity.Current != null)
                    {
                        foreach(var tag in Activity.Current.Tags)
                        {
                            Console.WriteLine($"{tag.Key} - {tag.Value}");
                        }
                    }

                    break;

                case "ActivityClass_DiagnosticSource.ActivityClass.StartGenerateRandom":
                    Console.WriteLine("StartGenerateRandom");
                    break;
                case "ActivityClass_DiagnosticSource.ActivityClass.EndGenerateRandom":
                    var randomValue = keyValue.Value.GetType().GetTypeInfo().GetDeclaredProperty("RandomNumber")?.
                        GetValue(keyValue.Value);

                    Console.WriteLine($"StopGenerateRandom Generated random value: {randomValue}");
                    break;
                default:
                    break;
            }
        }
    }
    class Subscriber : IObserver<DiagnosticListener>
    {
        public void OnCompleted()
        {}
        public void OnError(Exception error)
        {}
        public void OnNext(DiagnosticListener listener)
        {
            if (listener.Name == typeof(ActivityClass).FullName)
            {
                listener.Subscribe(new MyLibraryListener());
            }

            if(listener.Name == "HttpHandlerDiagnosticListener")
            {
                listener.Subscribe(new HttpClientObserver());
            }
        }

                                //HttpClient - DiagnosticSource

                                //Sends event when the HTTP Request starts stops

                                //System.Net.HttpRequest/System.Net.HttpResponse: Deprecated

                                //System.Net.Http.HttpRequest.Start/System.Net.Http.HttpRequest.Stop:Based on Activity, use these.


        private class HttpClientObserver : IObserver<KeyValuePair<string, object>>
        {
            Stopwatch _stopwatch = new Stopwatch();
            public void OnCompleted()
            {}
            public void OnError(Exception error)
            {}
            public void OnNext(KeyValuePair<string, object> receivedEvent)
            {
                switch (receivedEvent.Key)
                {
                    case "System.Net.Http.HttpRequestOut.Start":
                        _stopwatch.Start();

                        if (receivedEvent.Value.GetType().GetTypeInfo().GetDeclaredProperty("Request")?.
                            GetValue(receivedEvent.Value) is HttpRequestMessage requestMessage)
                        {
                            Console.WriteLine($"Http Request Start: {requestMessage.Method} -" + $"{requestMessage.RequestUri} - parentactivity Id: {Activity.Current.ParentId}");
                        }
                        break;
                    case "System.Net.Http.HttpRequestOut.Stop":
                        _stopwatch.Stop();

                        if(receivedEvent.Value.GetType().GetTypeInfo().GetDeclaredProperty("Response")?.
                            GetValue(receivedEvent.Value) is HttpResponseMessage responseMessage)
                        {
                            Console.WriteLine($"Http Request finished: took " + $"{responseMessage.StatusCode} - parentactivity Id: {Activity.Current.ParentId}");
                        }
                        break;
                }
            }
        }
    }
}
