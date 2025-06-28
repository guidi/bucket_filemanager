using Amazon.S3;
using Amazon.S3.Model;
using BucketFileManager.WPF.Configuracao;
using System.Collections.Generic;
using System.Linq;

namespace BucketFileManager.WPF.Provedor;

public class ServicoBuckets
{
    public async Task<List<string>> ListarAsync(ConfiguracaoApp config)
    {
        if (config.BucketUnico && !string.IsNullOrWhiteSpace(config.NomeBucket))
        {
            return new List<string> { config.NomeBucket };
        }

        return config.Provedor == TipoProvedor.BackblazeB2
            ? await ListarB2Async(config.ServiceUrl!, config.ApplicationKeyId!, config.ApplicationKey!)
            : await ListarS3Async(config.Endpoint!, config.AccessKey!, config.SecretKey!);
    }

    private async Task<List<string>> ListarB2Async(string serviceUrl, string keyId, string key)
    {
        using var client = ClienteS3Factory.CriarParaB2(serviceUrl, keyId, key);
        var resposta = await client.ListBucketsAsync();
        return resposta.Buckets.Select(b => b.BucketName).ToList();
    }

    private async Task<List<string>> ListarS3Async(string endpoint, string accessKey, string secretKey)
    {
        using var client = ClienteS3Factory.CriarParaS3(endpoint, accessKey, secretKey);
        var resposta = await client.ListBucketsAsync();
        return resposta.Buckets.Select(b => b.BucketName).ToList();
    }
}
