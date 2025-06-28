using System.Text.Json.Serialization;

namespace BucketFileManager.WPF.Configuracao;

public class ConfiguracaoApp
{
    public TipoProvedor Provedor { get; set; }
    public string? ApplicationKeyId { get; set; }
    public string? ApplicationKey { get; set; }
    public string? ServiceUrl { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string? Endpoint { get; set; }
    public bool BucketUnico { get; set; }
    public string? NomeBucket { get; set; }
}
