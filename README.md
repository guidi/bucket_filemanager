# bucket_filemanager
Gerenciador de arquivos em WPF para buckets do Backblaze B2 e de provedores compatíveis com S3.
Todas as interações com os provedores utilizam a biblioteca **AWS SDK for .NET**, inclusive o acesso ao Backblaze B2 por meio do endpoint S3 compatível.

## Stack Utilizada
- **.NET 9.0** com **WPF**
- **C#**
- **AWS SDK for .NET**

O layout do aplicativo foi baseado no proposto no vídeo ["WPF - Page Navigation / Switching Views | MVVM | Dashboard | UI Design | XAML | C# | Tutorial"](https://www.youtube.com/watch?v=CkHyDYeImjY), adaptado para o tema escuro do projeto.

## Configuração inicial
Ao abrir o aplicativo pela primeira vez é exibida uma janela solicitando o tipo de provedor e as chaves de acesso. Quando o provedor escolhido é o Backblaze B2 também é necessário informar a **Service URL** do bucket. Existe ainda a opção **Bucket único** para usar chaves restritas a um único bucket. Nesse caso informe o nome do bucket e o teste de conexão apenas lista os arquivos desse bucket. As informações são validadas pelo botão **Testar** e gravadas em arquivo local criptografado.

## Carregamento de buckets
Após a configuração o aplicativo lista todos os buckets disponíveis do provedor escolhido. Se a opção **Bucket único** estiver marcada é exibido somente o bucket informado. Cada bucket aparece como um nó da árvore de navegação e o botão **Carregar** permite atualizar a lista caso as chaves sejam alteradas. A tela principal também mostra o caminho do arquivo de configuração utilizado.

## Navegação de arquivos
Ao expandir um bucket a aplicação carrega pastas e arquivos usando o AWS SDK. A lista é mostrada com colunas de nome, tamanho e data. Arquivos são baixados com duplo clique ou menu de contexto e novos itens podem ser enviados pelo botão **Upload**. A barra de endereço indica o caminho atual e o progresso das transferências aparece no rodapé.
O tamanho dos arquivos utiliza unidades como KB ou MB e é possível excluir itens com confirmação. A barra de progresso exibe o percentual durante uploads e downloads.

## Cache em memória
Os diretórios já consultados ficam armazenados para evitar novas chamadas. O botão **Atualizar** remove apenas o cache do caminho aberto e recarrega as informações exibindo uma barra de progresso durante a operação.
