# elaw_webcrawler
Criar ferramenta para extração dos dados de website (Webcrawler).

## Requisitos
<ol>
<li>Acessar o site "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc".</li>
<li>Extrair os campos "IP Adress", "Port", "Country" e "Protocol". de todas as linhas, de todas as páginas disponíveis na execução.</li>
<li>Necessário salvar o resultado da extração em arquivo json, que deverá ser salvo na máquina.</li>
<li>Necessário salvar em banco de dados a data início execução, data termino execução, quantidade de páginas, quantidade linhas extraídas em todas as páginas e arquivo json gerado.</li>
<li>Necessário print (arquivo .html) de cada página.</li>
<li>Necessário que o webcrawler seja multithread, com máximo de 3 execuções simultâneas.</li>
</ol>

