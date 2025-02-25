# BoardGameGeekLike

# Exercício Api - Bgg Like

Api de um site especializado em jogos de tabuleiro onde os usuários podem se registrar, dar uma nota aos jogos, cadastrar partidas e buscar por jogos para conferir suas estatísticas.

# Requisitos da Aplicação

### ETAPA 1 - Funções do Administrador

- Criar categorias de jogo, ex Eurogamer, Party, etc
- Editar de categoria
- Remover de categoria
- Cadastro do jogo (nome, descrição, nr min-max de jogadores, idade mínima, categoria)
- Edição do jogo
- Remoção do jogo

### ETAPA 2 - Funções de Usuário

- Cadastro de usuário (nickname, email, data de nascimento)
- Edição perfil de usuário
- Exluir conta de usuário
- Votar em um jogo (informar nota de 0 a 5), o voto é único
- Cadastrar sessão de jogo (data, número de jogadores que participaram e duração da partida)
- Editar sessão de jogo
- Excluir sessão de jogo

### ETAPA 3 - Seed

- Cadastrar 10 categorias com nomes aleatórios
- Cadastrar 10 jogos com nomes aleatórios em cada categoria
- Cadastrar 200 jogadores com dados aleatórios
- Cadastrar de 0 a 30 sessões aleatórias de jogos em cada jogador (as sessões podem ser em jogos repetidos)
- Cada jogador deverá votar com uma nota aleatória em cada um dos jogos dos quais ele tem pelo menos 1 sessão registrada

### ETAPA 4 - Funções de Navegação

- Listar todos os jogos com a possibildiade de filtrar:
    - Filtro por nome
    - Filtro por nr de jogadores (ex jogos que aceitam pelo menos 3 jogadores)
    - Filtro por idade mímina
    - Filtro por categoria de jogo
    - Filtro por nota do jogo
- Ver detalhes de um jogo
    - nome, descrição, categoria, nr min-max de jogadores, idade mínima,
    - nr de sessões jogadas
    - tempo médio de partida
    - nota média do jogo
    - Lista com as últimas 5 sessões jogadas (nickname do usuário, data, quantidade de jogadores que participara e duração da sessão

### ETAPA 5 - Estatísticas

- Ranking do jogos
    - 3 jogos mais jogados
    - 3 jogos com melhor nota
    - 3 jogos mais curtos
    - 3 jogos mais demorados
    - 3 jogos que os adultos mais gostam
    - 3 jogos que os menores de idade mais gostam
- Ranking das categorias
    - 3 categorias mais jogadas
    - 3 categorias que os jogos tem as melhores notas
    - 3 categorias dos jogos mais demorados
    - 3 categorias dos jogos mais curtos
