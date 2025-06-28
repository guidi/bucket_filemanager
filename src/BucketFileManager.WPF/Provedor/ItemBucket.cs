using System.IO;

namespace BucketFileManager.WPF.Provedor;

public class ItemBucket
{
    public required string Nome { get; init; }
    public long Tamanho { get; init; }
    public DateTime Data { get; init; }
    public bool EhPasta { get; init; }

    public string NomeArquivo
    {
        get
        {
            return Path.GetFileName(Nome.TrimEnd('/'));
        }
    }

    public string TamanhoFormatado
    {
        get
        {
            if (EhPasta || Tamanho == 0)
            {
                return "-";
            }

            var tamanhos = new[] { "B", "KB", "MB", "GB", "TB" };
            double valor = Tamanho;
            var indice = 0;
            while (valor >= 1024 && indice < tamanhos.Length - 1)
            {
                valor /= 1024;
                indice++;
            }

            return $"{valor:0.#} {tamanhos[indice]}";
        }
    }
}
