using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using BucketFileManager.WPF.Configuracao;
using System.Collections.Generic;
using System.Linq;

namespace BucketFileManager.WPF.Provedor;

public class ServicoArquivos
{
    public async Task<List<ItemBucket>> ListarAsync(ConfiguracaoApp config, string bucket, string prefixo)
    {
        using var client = ClienteS3Factory.Criar(config);

        var request = new ListObjectsV2Request
        {
            BucketName = bucket,
            Prefix = prefixo,
            Delimiter = "/"
        };

        var resposta = await client.ListObjectsV2Async(request);
        var itens = new List<ItemBucket>();

        foreach (var pasta in resposta.CommonPrefixes)
        {
            itens.Add(new ItemBucket
            {
                Nome = pasta,
                Tamanho = 0,
                Data = DateTime.MinValue,
                EhPasta = true
            });
        }

        foreach (var obj in resposta.S3Objects)
        {
            if (obj.Key == prefixo)
            {
                continue;
            }

            itens.Add(new ItemBucket
            {
                Nome = obj.Key,
                Tamanho = obj.Size,
                Data = obj.LastModified,
                EhPasta = false
            });
        }

        return itens.OrderBy(i => i.Nome).ToList();
    }

    public async Task DownloadAsync(ConfiguracaoApp config, string bucket, string chave, string destino, IProgress<double> progresso)
    {
        using var client = ClienteS3Factory.Criar(config);
        var transfer = new TransferUtility(client);
        var requisicao = new TransferUtilityDownloadRequest
        {
            BucketName = bucket,
            Key = chave,
            FilePath = destino
        };
        requisicao.WriteObjectProgressEvent += (_, args) =>
        {
            if (args.TotalBytes > 0)
            {
                progresso.Report(args.TransferredBytes / (double)args.TotalBytes);
            }
        };

        await transfer.DownloadAsync(requisicao);
    }

    public async Task UploadAsync(ConfiguracaoApp config, string bucket, string chave, string caminho, IProgress<double> progresso)
    {
        using var client = ClienteS3Factory.Criar(config);
        var transfer = new TransferUtility(client);
        var requisicao = new TransferUtilityUploadRequest
        {
            BucketName = bucket,
            Key = chave,
            FilePath = caminho
        };
        requisicao.UploadProgressEvent += (_, args) =>
        {
            if (args.TotalBytes > 0)
            {
                progresso.Report(args.TransferredBytes / (double)args.TotalBytes);
            }
        };

        await transfer.UploadAsync(requisicao);
    }

    public async Task ExcluirAsync(ConfiguracaoApp config, string bucket, string chave)
    {
        using var client = ClienteS3Factory.Criar(config);
        var request = new DeleteObjectRequest
        {
            BucketName = bucket,
            Key = chave
        };

        await client.DeleteObjectAsync(request);
    }

}
