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

Migrations using VS Code:
dotnet ef migrations add <MigrationTitle>
dotnet ef database update

Game Mechanics:
Undetermined = 0,
ActionPoints = 1, // Players have a limited number of actions to take each turn (e.g., Puerto Rico).
AreaControl = 2, // Players vie for control of areas on a board (e.g., Risk).
CardDrafting = 3, // Players select cards from a hand to play (e.g., 7 Wonders).
CooperativePlay = 4, // Players work together to achieve a common goal (e.g., Pandemic).
DeckBuilding = 5, // Players build a deck of cards to use during the game (e.g., Aeon's End).
DiceRolling = 6, // Players roll dice to determine outcomes (e.g., Monopoly).
RolePlaying = 7, // Players assume roles and act out scenarios (e.g., Dungeons & Dragons).
SetCollection = 8, // Players gather specific sets of items for points (e.g., Ticket to Ride).
TilePlacement = 9, // Players place tiles to form a board or pattern (e.g., Azul).
WorkerPlacement = 10, // Players assign tokens to take specific actions (e.g., Agricola).

### Medieval Auto Battler

    -Campaign:
        -Only one campaign at a time is allowed;
        -If a new campaign gets started before finishing the current one, the current gets hard deleted along with its battles and duels;
        -When a player finishes all quests, the campaign is finished;
        -If the player at any point owns a negative gold stash, the campaign is lost;

    -Battle:
        -If a player leaves the battle, that battle and its duels will be hard deleted when a new once is started;
        -If a player retreates a battle, its duels are deleted but the battle is recorded and the player looses 5 coins;

    -Duels:
        -A duel involves an attacking card and a defending card;
        -The attacking card may have a total power greated than it's power if its type has an upper hand against the defend card type;
        -The defending card never has a total power, it defends only with its power;
        -Duels result in a duel points, earned xp and bonus xp;
        -If duel points are positive, attacker wins the duel otherwise attacker he looses;
        -Duel points are thus calculated:
            =>attacking card full power - defending card power;
            or (if type of the attacking card has no upper hand againd the defending card type):
            =>attacking card power - defending card power;
        -Duel points can be either positive or negative;
        -The player always looses ties;
        -The result earned xp is never a negative value (always 0 or greater than 0);
        -Earned xp is thus calculated:
            => earnedXp = basisXp + duelPoints - xpPenalty;
            Where:
                => basisXp = 1;
                => xpPenalty = lvlDif * 10
                => lvlDif = npcLevel - playerLevel
        -Bonus xp is always rounded up and is thus calculated:
            => bonusXp = earnedXp * (1 / (playerState + lvlDif))
            Where playerState is a number between 1 and 8

    -Cards:
        -There are 4 types of cards: Neutral, Ranged, Infantry and Cavalry;
        -All cards have a power value, an upper hand value and a total power;
        -The power value and upper hand value varies from 0 up to 9;
        -There are 1 card of each combination (one Neutral 0|0, on Infantry 0|0 and so on)
        -Upper Hand is applied following these rules:
            -Neutral > Neutral;
            -Ranged, Infantry and Cavalry > Neutral;
            -Ranged > Infantry;
            -Infantry > Cavalry
            -Cavalry > Ranged
        -Total Power value always applies only for the attacker and is equal to the sum of the attacking card's Power and Upper Hand;

    -Special Cards:
        Truce: Neutral 0|0 => when played cancel any opposing card, duel results will be: 0 points and 0 xp;
        Pickaxe: Infantry 0|0, 1|1, ..., 9|9 => this is the card used in the mine, the more powerfull the card is the greater are the chances of getting better raw materials;
