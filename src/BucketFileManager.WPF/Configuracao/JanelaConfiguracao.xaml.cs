using Amazon.S3;
using Amazon.S3.Model;
using BucketFileManager.WPF.Provedor;
using System.Windows;
using System.Windows.Controls;

namespace BucketFileManager.WPF.Configuracao;

public partial class JanelaConfiguracao : Window
{
    private readonly ServicoConfiguracao servico;

    public JanelaConfiguracao(ServicoConfiguracao servico)
    {
        InitializeComponent();
        this.servico = servico;
    }

    private void OnBucketUnicoChecked(object sender, RoutedEventArgs e)
    {
        if (painelBucket is null)
        {
            return;
        }

        painelBucket.Visibility = chkBucketUnico.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        btnSalvar.IsEnabled = false;
    }

    private void OnProvedorAlterado(object sender, SelectionChangedEventArgs e)
    {
        if (painelB2 is null || painelS3 is null || btnSalvar is null)
        {
            // Evento disparado durante a inicialização antes dos controles serem criados
            return;
        }

        if (comboProvedor.SelectedIndex == 0)
        {
            painelB2.Visibility = Visibility.Visible;
            painelS3.Visibility = Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(txtB2ServiceUrl.Text))
            {
                txtB2ServiceUrl.Text = "https://s3.us-east-005.backblazeb2.com";
            }
        }
        else
        {
            painelB2.Visibility = Visibility.Collapsed;
            painelS3.Visibility = Visibility.Visible;
        }

        btnSalvar.IsEnabled = false;
    }

    private async void OnTestar(object sender, RoutedEventArgs e)
    {
        btnSalvar.IsEnabled = false;
        bool ok = false;
        if (comboProvedor.SelectedIndex == 0)
        {
            if (chkBucketUnico.IsChecked == true && !string.IsNullOrWhiteSpace(txtBucket.Text))
            {
                ok = await TestarB2BucketAsync(txtB2ServiceUrl.Text, txtApplicationKeyId.Text, txtApplicationKey.Password, txtBucket.Text);
            }
            else
            {
                ok = await TestarB2Async(txtB2ServiceUrl.Text, txtApplicationKeyId.Text, txtApplicationKey.Password);
            }
        }
        else
        {
            if (chkBucketUnico.IsChecked == true && !string.IsNullOrWhiteSpace(txtBucket.Text))
            {
                ok = await TestarS3BucketAsync(txtEndpoint.Text, txtAccessKey.Text, txtSecretKey.Password, txtBucket.Text);
            }
            else
            {
                ok = await TestarS3Async(txtEndpoint.Text, txtAccessKey.Text, txtSecretKey.Password);
            }
        }

        MessageBox.Show(ok ? "Chaves válidas" : "Falha na validação", "Teste", MessageBoxButton.OK, ok ? MessageBoxImage.Information : MessageBoxImage.Error);
        btnSalvar.IsEnabled = ok;
    }

    private async Task<bool> TestarB2Async(string serviceUrl, string keyId, string key)
    {
        try
        {
            using var client = ClienteS3Factory.CriarParaB2(serviceUrl, keyId, key);
            var resp = await client.ListBucketsAsync();
            return resp.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> TestarB2BucketAsync(string serviceUrl, string keyId, string key, string bucket)
    {
        try
        {
            using var client = ClienteS3Factory.CriarParaB2(serviceUrl, keyId, key);
            var request = new ListObjectsV2Request { BucketName = bucket, MaxKeys = 1 };
            var resp = await client.ListObjectsV2Async(request);
            return resp.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> TestarS3Async(string endpoint, string accessKey, string secretKey)
    {
        try
        {
            using var client = ClienteS3Factory.CriarParaS3(endpoint, accessKey, secretKey);
            var resp = await client.ListBucketsAsync();
            return resp.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> TestarS3BucketAsync(string endpoint, string accessKey, string secretKey, string bucket)
    {
        try
        {
            using var client = ClienteS3Factory.CriarParaS3(endpoint, accessKey, secretKey);
            var request = new ListObjectsV2Request { BucketName = bucket, MaxKeys = 1 };
            var resp = await client.ListObjectsV2Async(request);
            return resp.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch
        {
            return false;
        }
    }

    private async void OnSalvar(object sender, RoutedEventArgs e)
    {
        var config = new ConfiguracaoApp();
        config.Provedor = comboProvedor.SelectedIndex == 0 ? TipoProvedor.BackblazeB2 : TipoProvedor.S3;
        if (config.Provedor == TipoProvedor.BackblazeB2)
        {
            config.ServiceUrl = txtB2ServiceUrl.Text;
            config.ApplicationKeyId = txtApplicationKeyId.Text;
            config.ApplicationKey = txtApplicationKey.Password;
        }
        else
        {
            config.Endpoint = txtEndpoint.Text;
            config.AccessKey = txtAccessKey.Text;
            config.SecretKey = txtSecretKey.Password;
        }
        config.BucketUnico = chkBucketUnico.IsChecked == true;
        config.NomeBucket = txtBucket.Text;
        await servico.SalvarAsync(config);
        DialogResult = true;
    }
}
