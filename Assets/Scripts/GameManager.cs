using System.Collections; // Necessário para criar os efeitos de tempo (Coroutines)
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Gerenciamento de Telas")]
    public GameObject telaMenu;
    public GameObject telaJogo;
    public GameObject telaFinal;
    
    [Header("Efeitos Visuais")]
    public CanvasGroup painelFade; // O painel preto que criamos
    public float tempoFade = 0.5f; // Duração do escurecimento
    public float velocidadeDigitacao = 0.03f; // Tempo entre cada letra no final

    [Header("Áudio")]
    public AudioSource fonteMusica;
    public AudioSource fonteEfeitos;
    public AudioClip musicaMenuEFinal;
    public AudioClip musicaJogo;
    public AudioClip somClique;

    [Header("Atributos do Jogador")]
    private int notas = 50;
    private int cansaco = 50;
    private int socializacao = 50;
    private int estresse = 50;

    [Header("Configuração de Jogo")]
    public QuestionData[] questions;
    private int currentQuestionIndex = 0;
    private bool emTransicao = false; // Trava os botões enquanto a tela pisca

    [Header("Referências da Interface (UI)")]
    public Image fundoImagem;
    public TextMeshProUGUI textoNotas;
    public TextMeshProUGUI textoCansaco;
    public TextMeshProUGUI textoSocializacao;
    public TextMeshProUGUI textoEstresse;

    [Header("Referências da Tela Final")]
    public Image fundoFinalImagem;
    public Sprite spriteAprovado;
    public Sprite spriteReprovado;
    public TextMeshProUGUI textoDiagnostico;

    void Start()
    {
        telaMenu.SetActive(true);
        telaJogo.SetActive(false);
        telaFinal.SetActive(false);
        painelFade.alpha = 0f;
        painelFade.blocksRaycasts = false;

        TocarMusica(musicaMenuEFinal);
    }

    // --- FUNÇÕES DE CLIQUE DOS BOTÕES ---

    public void IniciarJogo()
    {
        if (emTransicao) return; // Ignora o clique se já estiver mudando de tela
        TocarSomClique();
        StartCoroutine(TransacaoParaJogo());
    }

    public void SairDoJogo()
    {
        TocarSomClique();
        Application.Quit();
    }

    public void ChooseSim()
    {
        if (emTransicao) return;
        TocarSomClique();
        ApplyEffects(questions[currentQuestionIndex].simNotas, questions[currentQuestionIndex].simCansaco, 
                     questions[currentQuestionIndex].simSocializacao, questions[currentQuestionIndex].simEstresse);
        StartCoroutine(ProximaPerguntaFade());
    }

    public void ChooseNao()
    {
        if (emTransicao) return;
        TocarSomClique();
        ApplyEffects(questions[currentQuestionIndex].naoNotas, questions[currentQuestionIndex].naoCansaco, 
                     questions[currentQuestionIndex].naoSocializacao, questions[currentQuestionIndex].naoEstresse);
        StartCoroutine(ProximaPerguntaFade());
    }

    public void ReiniciarJogo()
    {
        if (emTransicao) return;
        TocarSomClique();

        notas = 50;
        cansaco = 50;
        socializacao = 50;
        estresse = 50;
        currentQuestionIndex = 0;

        StartCoroutine(TransacaoParaMenu());
    }

    // --- LÓGICA DE STATUS ---

    private void ApplyEffects(int n, int c, int s, int e)
    {
        notas = Mathf.Clamp(notas + n, 0, 100);
        cansaco = Mathf.Clamp(cansaco + c, 0, 100);
        socializacao = Mathf.Clamp(socializacao + s, 0, 100);
        estresse = Mathf.Clamp(estresse + e, 0, 100);
        UpdateUI();
    }

    private void UpdateUI()
    {
        textoNotas.text = notas.ToString();
        textoCansaco.text = cansaco.ToString();
        textoSocializacao.text = socializacao.ToString();
        textoEstresse.text = estresse.ToString();
    }

    private void LoadQuestion()
    {
        fundoImagem.sprite = questions[currentQuestionIndex].backgroundSprite;
    }

    // --- ANIMAÇÕES (COROUTINES) ---

    private IEnumerator TransacaoParaJogo()
    {
        emTransicao = true;
        painelFade.blocksRaycasts = true; // Bloqueia cliques na tela

        // 1. Escurece a tela gradualmente
        while (painelFade.alpha < 1f)
        {
            painelFade.alpha += Time.deltaTime / tempoFade;
            yield return null; // Espera o próximo frame
        }

        // 2. Troca tudo enquanto a tela está preta
        telaMenu.SetActive(false);
        telaJogo.SetActive(true);
        TocarMusica(musicaJogo);
        UpdateUI();
        LoadQuestion();

        // 3. Clareia a tela gradualmente
        while (painelFade.alpha > 0f)
        {
            painelFade.alpha -= Time.deltaTime / tempoFade;
            yield return null;
        }

        painelFade.blocksRaycasts = false; // Libera os cliques
        emTransicao = false;
    }

    private IEnumerator ProximaPerguntaFade()
    {
        emTransicao = true;
        painelFade.blocksRaycasts = true;
        currentQuestionIndex++;

        if (currentQuestionIndex < questions.Length)
        {
            while (painelFade.alpha < 1f) { painelFade.alpha += Time.deltaTime / tempoFade; yield return null; }
            
            LoadQuestion(); // Troca a imagem da semana no escuro
            
            while (painelFade.alpha > 0f) { painelFade.alpha -= Time.deltaTime / tempoFade; yield return null; }
            
            painelFade.blocksRaycasts = false;
            emTransicao = false;
        }
        else
        {
            StartCoroutine(TransacaoParaFinal());
        }
    }

    private IEnumerator TransacaoParaFinal()
    {
        while (painelFade.alpha < 1f) { painelFade.alpha += Time.deltaTime / tempoFade; yield return null; }

        telaJogo.SetActive(false);
        telaFinal.SetActive(true);
        TocarMusica(musicaMenuEFinal);

        if (notas >= 60) fundoFinalImagem.sprite = spriteAprovado;
        else fundoFinalImagem.sprite = spriteReprovado;

        textoDiagnostico.text = ""; // Limpa o quadro branco

        while (painelFade.alpha > 0f) { painelFade.alpha -= Time.deltaTime / tempoFade; yield return null; }

        // Diagnóstico Final gerado
        string diagCansaco = cansaco > 60 ? "- Seu corpo está exausto, você precisa dormir por uma semana.\n\n" : "- Você terminou o semestre com energia de sobra.\n\n";
        string diagSocial = socializacao > 60 ? "- Você se tornou a alma do campus e fez muitos amigos!\n\n" : "- Você virou um fantasma nos corredores da faculdade.\n\n";
        string diagEstresse = estresse > 60 ? "- Seu nível de estresse foi ao teto, você vai precisar de férias das férias." : "- Você lidou com tudo de forma muito calma e plena.";
        
        string textoFinal = diagCansaco + diagSocial + diagEstresse;

        // Inicia o efeito de máquina de escrever
        StartCoroutine(EfeitoDigitacao(textoFinal));
    }

    private IEnumerator EfeitoDigitacao(string textoCompleto)
    {
        // Pega letra por letra e adiciona no quadro branco com um pequeno delay
        foreach (char letra in textoCompleto.ToCharArray())
        {
            textoDiagnostico.text += letra;
            yield return new WaitForSeconds(velocidadeDigitacao);
        }

        painelFade.blocksRaycasts = false;
        emTransicao = false;
    }

    private IEnumerator TransacaoParaMenu()
    {
        emTransicao = true;
        painelFade.blocksRaycasts = true;

        while (painelFade.alpha < 1f) { painelFade.alpha += Time.deltaTime / tempoFade; yield return null; }

        telaFinal.SetActive(false);
        telaMenu.SetActive(true);

        while (painelFade.alpha > 0f) { painelFade.alpha -= Time.deltaTime / tempoFade; yield return null; }

        painelFade.blocksRaycasts = false;
        emTransicao = false;
    }

    // --- MÉTODOS DE ÁUDIO ---

    private void TocarMusica(AudioClip novaMusica)
    {
        if (fonteMusica.clip == novaMusica) return; 
        fonteMusica.clip = novaMusica;
        fonteMusica.Play();
    }

    private void TocarSomClique()
    {
        fonteEfeitos.PlayOneShot(somClique);
    }
}