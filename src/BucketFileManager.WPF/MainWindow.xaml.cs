using System.Windows;
using System.Windows.Controls;
using BucketFileManager.WPF.Configuracao;
using BucketFileManager.WPF.Provedor;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BucketFileManager.WPF;

public partial class MainWindow : Window
{
    private readonly ServicoConfiguracao servicoConfiguracao;
    private readonly ServicoBuckets servicoBuckets;
    private readonly ServicoArquivos servicoArquivos;
    private readonly Dictionary<string, List<ItemBucket>> cache;
    private ConfiguracaoApp? configuracao;

    public MainWindow(ServicoConfiguracao servicoConfiguracao)
    {
        InitializeComponent();
        this.servicoConfiguracao = servicoConfiguracao;
        servicoBuckets = new ServicoBuckets();
        servicoArquivos = new ServicoArquivos();
        cache = new Dictionary<string, List<ItemBucket>>();
    }

    public async Task InicializarAsync()
    {
        configuracao = await servicoConfiguracao.CarregarAsync();
        lblCaminho.Text = servicoConfiguracao.CaminhoArquivo;
        await CarregarBucketsAsync();
    }

    private async void OnCarregar(object sender, RoutedEventArgs e)
    {
        configuracao = await servicoConfiguracao.CarregarAsync();
        lblCaminho.Text = servicoConfiguracao.CaminhoArquivo;
        await CarregarBucketsAsync();
    }

    private async Task CarregarBucketsAsync()
    {
        if (configuracao == null)
        {
            return;
        }

        arvoreBuckets.Items.Clear();
        cache.Clear();
        listaArquivos.ItemsSource = null;
        barraEndereco.Text = string.Empty;
        var buckets = await servicoBuckets.ListarAsync(configuracao);
        foreach (var nome in buckets)
        {
            var item = new TreeViewItem { Header = nome, Tag = nome };
            item.Expanded += OnExpandir;
            item.Style = (Style)FindResource("DarkTreeViewItemStyle");
            arvoreBuckets.Items.Add(item);
        }
    }

    private async void OnExpandir(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (configuracao == null)
        {
            return;
        }

        if (sender is not TreeViewItem item || item.Tag is not string caminho)
        {
            return;
        }

        await ExpandirAsync(item, caminho);
    }

    private async Task ExpandirAsync(TreeViewItem item, string caminho)
    {
        if (cache.ContainsKey(caminho))
        {
            PopularArvore(item, caminho, cache[caminho]);
            if (Equals(arvoreBuckets.SelectedItem, item))
            {
                listaArquivos.ItemsSource = cache[caminho].Where(i => !i.EhPasta).ToList();
            }
            return;
        }

        barraProgresso.Visibility = Visibility.Visible;
        lblProgresso.Text = string.Empty;
        var partes = caminho.Split('/', 2);
        var bucket = partes[0];
        var prefixo = partes.Length > 1 ? partes[1] : string.Empty;
        if (!string.IsNullOrEmpty(prefixo) && !prefixo.EndsWith("/"))
        {
            prefixo += "/";
        }
        var itens = await servicoArquivos.ListarAsync(configuracao!, bucket, prefixo);
        cache[caminho] = itens;
        PopularArvore(item, caminho, itens);
        if (Equals(arvoreBuckets.SelectedItem, item))
        {
            listaArquivos.ItemsSource = itens.Where(i => !i.EhPasta).ToList();
        }
        barraProgresso.Visibility = Visibility.Collapsed;
        lblProgresso.Text = string.Empty;
    }

    private void PopularArvore(TreeViewItem item, string caminho, IEnumerable<ItemBucket> itens)
    {
        item.Items.Clear();
        foreach (var pasta in itens.Where(i => i.EhPasta))
        {
            var bucket = caminho.Split('/')[0];
            var nomePasta = pasta.Nome.TrimEnd('/').Split('/').Last();
            var sub = new TreeViewItem
            {
                Header = nomePasta,
                Tag = $"{bucket}/{pasta.Nome}",
                Style = (Style)FindResource("DarkTreeViewItemStyle")
            };
            sub.Expanded += OnExpandir;
            item.Items.Add(sub);
        }
    }

    private async void OnCaminhoSelecionado(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (configuracao == null)
        {
            return;
        }

        if (arvoreBuckets.SelectedItem is not TreeViewItem item || item.Tag is not string caminho)
        {
            return;
        }

        barraEndereco.Text = caminho;
        if (!cache.ContainsKey(caminho))
        {
            await ExpandirAsync(item, caminho);
        }

        if (cache.TryGetValue(caminho, out var itens))
        {
            listaArquivos.ItemsSource = itens.Where(i => !i.EhPasta).ToList();
        }
    }

    private void OnAtualizar(object sender, RoutedEventArgs e)
    {
        if (arvoreBuckets.SelectedItem is not TreeViewItem item || item.Tag is not string caminho)
        {
            return;
        }

        cache.Remove(caminho);
        OnExpandir(item, new RoutedEventArgs(TreeViewItem.ExpandedEvent));
    }

    private async void OnDuploCliqueArquivo(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (listaArquivos.SelectedItem is not ItemBucket arquivo || arquivo.EhPasta)
        {
            return;
        }

        await BaixarArquivoAsync(arquivo);
    }

    private async void OnDownload(object sender, RoutedEventArgs e)
    {
        if (listaArquivos.SelectedItem is not ItemBucket arquivo || arquivo.EhPasta)
        {
            return;
        }

        await BaixarArquivoAsync(arquivo);
    }

    private async Task BaixarArquivoAsync(ItemBucket arquivo)
    {
        if (configuracao == null)
        {
            return;
        }

        var dlg = new SaveFileDialog { FileName = Path.GetFileName(arquivo.Nome) };
        if (dlg.ShowDialog() != true)
        {
            return;
        }

        barraProgresso.Visibility = Visibility.Visible;
        barraProgresso.Value = 0;
        lblProgresso.Text = "0%";
        var progresso = new Progress<double>(p =>
        {
            barraProgresso.Value = p * 100;
            lblProgresso.Text = $"{barraProgresso.Value:0}%";
        });

        var partes = barraEndereco.Text.Split('/', 2);
        var bucket = partes[0];
        await servicoArquivos.DownloadAsync(configuracao, bucket, arquivo.Nome, dlg.FileName, progresso);

        barraProgresso.Visibility = Visibility.Collapsed;
        lblProgresso.Text = string.Empty;
        MessageBox.Show("Download concluído", "Download", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void OnUpload(object sender, RoutedEventArgs e)
    {
        if (configuracao == null)
        {
            return;
        }

        if (arvoreBuckets.SelectedItem is not TreeViewItem item || item.Tag is not string caminho)
        {
            return;
        }

        var dlg = new OpenFileDialog();
        if (dlg.ShowDialog() != true)
        {
            return;
        }

        barraProgresso.Visibility = Visibility.Visible;
        barraProgresso.Value = 0;
        lblProgresso.Text = "0%";
        var progresso = new Progress<double>(p =>
        {
            barraProgresso.Value = p * 100;
            lblProgresso.Text = $"{barraProgresso.Value:0}%";
        });

        var partes = caminho.Split('/', 2);
        var bucket = partes[0];
        var prefixo = partes.Length > 1 ? partes[1] : string.Empty;
        var chave = string.IsNullOrEmpty(prefixo) ? Path.GetFileName(dlg.FileName) : $"{prefixo}{Path.GetFileName(dlg.FileName)}";

        await servicoArquivos.UploadAsync(configuracao, bucket, chave, dlg.FileName, progresso);
        cache.Remove(caminho);
        OnExpandir(item, new RoutedEventArgs(TreeViewItem.ExpandedEvent));
        barraProgresso.Visibility = Visibility.Collapsed;
        lblProgresso.Text = string.Empty;
        MessageBox.Show("Upload concluído", "Upload", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void OnExcluir(object sender, RoutedEventArgs e)
    {
        if (configuracao == null)
        {
            return;
        }

        if (listaArquivos.SelectedItem is not ItemBucket arquivo || arquivo.EhPasta)
        {
            return;
        }

        var confirmacao = MessageBox.Show($"Excluir '{Path.GetFileName(arquivo.Nome)}'?", "Excluir", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirmacao != MessageBoxResult.Yes)
        {
            return;
        }

        var partes = barraEndereco.Text.Split('/', 2);
        var bucket = partes[0];
        await servicoArquivos.ExcluirAsync(configuracao, bucket, arquivo.Nome);
        if (arvoreBuckets.SelectedItem is TreeViewItem item && item.Tag is string caminho)
        {
            cache.Remove(caminho);
            OnExpandir(item, new RoutedEventArgs(TreeViewItem.ExpandedEvent));
        }
    }

    private void OnFechar(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
