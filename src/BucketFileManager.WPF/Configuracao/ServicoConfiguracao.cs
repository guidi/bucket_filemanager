using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BucketFileManager.WPF.Configuracao;

public class ServicoConfiguracao
{
    private readonly string caminho;
    public string CaminhoArquivo => caminho;

    public ServicoConfiguracao()
    {
        var pasta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BucketFileManager");
        if (!Directory.Exists(pasta))
        {
            Directory.CreateDirectory(pasta);
        }
        caminho = Path.Combine(pasta, "config.dat");
    }

    public async Task<ConfiguracaoApp?> CarregarAsync()
    {
        if (!File.Exists(caminho))
        {
            return null;
        }
        var protegido = await File.ReadAllBytesAsync(caminho);
        var dados = ProtectedData.Unprotect(protegido, null, DataProtectionScope.CurrentUser);
        var json = Encoding.UTF8.GetString(dados);
        return JsonSerializer.Deserialize<ConfiguracaoApp>(json);
    }

    public async Task SalvarAsync(ConfiguracaoApp config)
    {
        var json = JsonSerializer.Serialize(config);
        var dados = Encoding.UTF8.GetBytes(json);
        var protegido = ProtectedData.Protect(dados, null, DataProtectionScope.CurrentUser);
        await File.WriteAllBytesAsync(caminho, protegido);
    }
}
