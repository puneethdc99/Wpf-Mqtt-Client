using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Diagnostics;
using MQTTnet.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;

namespace WpfMqttClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool withoutCert = false; 
        private IMqttClient client;
        IMqttNetLogger _clientLogger = new MqttNetLogger("client");


        public MainWindow()
        {
            InitializeComponent();
        }

        public void Connect()
        {

            try
            {
                bool con = false;
                if(Certificates.IsCertificateEnabled())
                    Certificates.GetCertificate("ABB Ability Edge");
                MqttFactory _mqttFactory = new MqttFactory(_clientLogger);
                var certificate = GetCert("ABB Ability Edge");
                if(certificate == null)
                {
                    certificate = new X509Certificate2(File.ReadAllBytes(this.certTxt.Text)); 
                }
                this.exMsg.Content = "";
                IMqttClientOptions options;
                MqttClientOptionsBuilder builderObj;
                builderObj = new MqttClientOptionsBuilder()
                              .WithClientId(Guid.NewGuid().ToString())
                              .WithCleanSession();

                if (this.layerCbox.SelectedItem.ToString().ToLower().Contains("tcp"))
                {
                    builderObj= builderObj
                    .WithClientId(Guid.NewGuid().ToString())
                    .WithTcpServer(this.ipTxt.Text, int.Parse(this.portTxt.Text))
                    .WithCleanSession();

                }
                else
                {
                    builderObj= builderObj
                    .WithClientId(Guid.NewGuid().ToString())
                    .WithWebSocketServer(this.ipTxt.Text)
                    .WithCleanSession().WithRequestProblemInformation(true);
                }

                if(!withoutCert)
                {
                    builderObj = builderObj
                   .WithTls(new MqttClientOptionsBuilderTlsParameters()
                   {
                       AllowUntrustedCertificates = true,
                       UseTls = true,
                       Certificates = new List<byte[]> { new X509Certificate2(certificate).Export(X509ContentType.Cert) },
                       CertificateValidationCallback = delegate { return true; },
                       IgnoreCertificateChainErrors = false,
                       IgnoreCertificateRevocationErrors = false
                   });
                }
                options = builderObj.Build();
                client = _mqttFactory.CreateMqttClient();



                var message = new MqttApplicationMessageBuilder()
        .WithTopic(this.topicTxt.Text)
        .WithPayload(this.msgTxt.Text + DateTime.Now.ToString())
        .WithExactlyOnceQoS()
        .WithRetainFlag()
        .Build();

                var result = client.ConnectAsync(options);
                if  (result != null)
                { MessageBox.Show(result.ToString()); }
                
                client.UseConnectedHandler(async e =>
                {
                    MessageBox.Show("Connected");
                    //this.connection_status_lbl.Content = "Connected";
                    con = true;
                    await client.PublishAsync(message);
                    MessageBox.Show("Published");

            });
                this.connection_status_lbl.Content = "";
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.exMsg.Content = ex.StackTrace + Environment.NewLine;
            }
        }

      

        private X509Certificate GetCert(string serialNumber)
        {
            try
            {
                X509Certificate FindCertificate(StoreLocation location)
                {
                    X509Store store = new X509Store(location);
                    store.Open(OpenFlags.OpenExistingOnly);
                    IEnumerable certs = store.Certificates.Find(X509FindType.FindBySubjectName, serialNumber, false);
                    return certs.OfType<X509Certificate>().FirstOrDefault();
                }
                return FindCertificate(StoreLocation.CurrentUser) ?? FindCertificate(StoreLocation.LocalMachine);
            }
            catch { return new X509Certificate(); }
        }



        private void DisConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                client.DisconnectAsync();
                this.connection_status_lbl.Content = "Dis-Connected";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Connection_Status_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (client.IsConnected)
                {
                    this.connection_status_lbl.Content = "Connected";
                }
                else
                {
                    this.connection_status_lbl.Content = "Not Connected";
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Connect_without_cert_Click(object sender, RoutedEventArgs e)
        {
            withoutCert = true;
            Connect();
        }

        private void Connect_with_cert_Click(object sender, RoutedEventArgs e)
        {
            withoutCert = false;
            Connect();
        }
    }
}

  
