using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Neko233.Qcp.Unity.Samples
{
    public sealed class QcpBasicClient : MonoBehaviour
    {
        [SerializeField] private string endpoint = "qcp://127.0.0.1:7000";

        private QcpClient client;

        private async void Start()
        {
            client = QcpClient.CreateDefault();
            await client.ConnectAsync(endpoint);
            await SendMoveAsync();
        }

        private Task SendMoveAsync()
        {
            var payload = Encoding.UTF8.GetBytes("move:0,0,0");
            return client.SendAsync(payload, QcpSendOptions.Realtime(latestKey: 1));
        }

        private async void OnDestroy()
        {
            if (client != null)
            {
                await client.CloseAsync();
                client.Dispose();
            }
        }
    }
}
