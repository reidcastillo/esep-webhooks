using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Function that processes a GitHub webhook payload and sends the issue URL to Slack.
        /// </summary>
        /// <param name="input">The JSON payload from the GitHub webhook.</param>
        /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
        /// <returns>A string response indicating the result of the function.</returns>
        public async Task<string> FunctionHandler(string input, ILambdaContext context)
        {
            try
            {
                // Deserialize the incoming JSON payload
                dynamic json = JsonConvert.DeserializeObject<dynamic>(input);

                // Extract the issue URL from the JSON
                string issueUrl = json.issue.html_url;

                // Format the message to send to Slack
                string payload = $"{{'text':'New GitHub Issue Created: {issueUrl}'}}";

                // Get the Slack URL from the environment variable
                var slackUrl = Environment.GetEnvironmentVariable("SLACK_URL");

                // Send the payload to Slack
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(slackUrl, content);

                // Log the response status for debugging
                context.Logger.LogLine($"Slack response status: {response.StatusCode}");

                return $"Processed issue URL: {issueUrl}";
            }
            catch (Exception ex)
            {
                // Log any errors for troubleshooting
                context.Logger.LogLine($"Error: {ex.Message}");
                return "Error processing the GitHub webhook payload";
            }
        }
    }
}
