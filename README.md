# trabalho-pratico-02

## 👨‍💻 Identificação

**Nome:** [Lin]  
**Número de Aluno:** [31469]  

## 🎮 Descrição do Jogo

Este jogo é um **Tower Defense simplificado de RTS like**, desenvolvido com o framework **MonoGame**, onde o jogador deve defender a base central de inimigos que surgem de fora do mapa e tentam alcançá-la.

O jogador controla uma ou mais **unidades defensoras** que automaticamente atacam inimigos próximos. Os inimigos podem ser de diferentes tipos, como:

- **Zombie**: se aproxima e ataca corpo a corpo.
- **Skeleton**: atira flechas a uma distância segura.

O objetivo é **sobreviver o maior tempo possível**, derrotando os inimigos antes que eles cheguem à base.

---

## 🛠️ Decisões de Implementação

- **Herança e polimorfismo** foram usados para reutilizar lógica entre diferentes herancas (`Enemy`, `Zombie`, `Skeleton`)(`Unit`, `Tank`, `Turret`,e ect).
- A **animação de personagens** é feita por ciclos de frames, com controle de tempo entre frames para animações suaves.
- A movimentação dos inimigos utiliza vetores direcionais com velocidade fixa.
- Os **ataques** foram implementados com temporizadores e colisões entre retângulos (`Bounds`).
- O sistema de **spawn de moedas** foi iniciado mas ainda pode ser expandido.
- Texturas foram organizadas por estados (Idle, Walk, Attack) para cada tipo de inimigo.
- Separação entre **Update** e **Draw** respeitando o ciclo de vida do MonoGame.

---

## 🕹️ Instruções de Jogo

- **Objetivo**: proteger a base central contra ondas de inimigos.
- Os inimigos aparecem nos cantos do mapa e caminham até a base.
- As unidades defensoras atacam automaticamente os inimigos próximos.
- Derrotar inimigos pode gerar **moedas** (a serem recolhidas por unidades).
- Se um inimigo atingir a base, o jogo poderá considera isso como "vida perdida".

### Controles
- Pressionar os botoes para criar unidades e clicar(arrastar se escolher unidades multiplas) para escolher unidades,rightclick para mandar unidade alcancar o destinno.
- Futuramente podem ser adicionados mais inimigos e unidades.


## 📂 Estrutura do Projeto

- `Game1.cs`: classe principal do jogo
- `Enemy.cs`: classe base para inimigos
- `Zombie.cs`: inimigo corpo-a-corpo
- `Skeleton.cs`: inimigo com ataque à distância
- `Arrow.cs`: classe para flechas disparadas por skeletons
- `Coin.cs`: moedas que surgem quando um inimigo morre
- `Content/`: contém todas as texturas e recursos usados
- `Camera.cs`: camara que age quando ponteiro arrasta
- `MainMenu.cs`: Ui da menu
- `GameUI.cs`: Ui do jogo

---

## ✅ Estado Atual

- ✅ Inimigos com IA básica (andar e atacar)
- ✅ Unidades defensoras com detecção de inimigos
- ✅ Sistema de animação por frames
- ✅ Colisões e ataques implementados
- ✅ Flechas com trajetória e dano
- ✅ Tela de menu ou fim de jogo e Pontuacao
- ✅ Com som ou música

---

## 📌 Melhorias Futuras

- Melhorar IA das unidades
- Criar sistema de níveis e dificuldade crescente
- Adicionar interface de jogador (UI)
- Mais Unidades e Inimigos
- Um organizacao melhor para codigos
