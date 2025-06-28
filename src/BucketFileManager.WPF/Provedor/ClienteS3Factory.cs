using Amazon.S3;
using BucketFileManager.WPF.Configuracao;

namespace BucketFileManager.WPF.Provedor;

public static class ClienteS3Factory
{
    public static AmazonS3Client Criar(ConfiguracaoApp config)
    {
        if (config.Provedor == TipoProvedor.BackblazeB2)
        {
            return CriarParaB2(config.ServiceUrl!, config.ApplicationKeyId!, config.ApplicationKey!);
        }

        return CriarParaS3(config.Endpoint!, config.AccessKey!, config.SecretKey!);
    }

    public static AmazonS3Client CriarParaB2(string serviceUrl, string keyId, string key)
    {
        var conf = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true,
            //https://xcp-ng.org/forum/topic/10541/backblaze-as-remote-error-unsupported-header-x-amz-checksum-mode-received-for-this-api-call.
            ResponseChecksumValidation = Amazon.Runtime.ResponseChecksumValidation.WHEN_REQUIRED,
            RequestChecksumCalculation = Amazon.Runtime.RequestChecksumCalculation.WHEN_REQUIRED
        };

        return new AmazonS3Client(keyId, key, conf);
    }

    public static AmazonS3Client CriarParaS3(string endpoint, string accessKey, string secretKey)
    {
        var conf = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true
        };

        return new AmazonS3Client(accessKey, secretKey, conf);
    }
}
