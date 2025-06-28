using System.Windows;
using BucketFileManager.WPF.Configuracao;

namespace BucketFileManager.WPF;

public partial class App : Application
{
    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        var servico = new ServicoConfiguracao();
        var config = await servico.CarregarAsync();
        if (config == null)
        {
            var janela = new JanelaConfiguracao(servico);
            var resultado = janela.ShowDialog();
            if (resultado != true)
            {
                Shutdown();
                return;
            }
            config = await servico.CarregarAsync();
        }

        var main = new MainWindow(servico);
        main.Show();
        await main.InicializarAsync();
    }
}
