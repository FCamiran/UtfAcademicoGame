using UnityEngine;

[CreateAssetMenu(fileName = "NovaPergunta", menuName = "JogoUTFPR/Pergunta")]
public class QuestionData : ScriptableObject
{
    [Header("Informações da Tela")]
    public string weekText; // Ex: "semana 1"
    public string hintText; // Dica ou contexto no quadro
    public string questionText; // A pergunta principal
    public Sprite backgroundSprite; // A arte de fundo/personagem

    [Header("Efeitos da Escolha: SIM")]
    public int simNotas;
    public int simCansaco;
    public int simSocializacao;
    public int simEstresse;

    [Header("Efeitos da Escolha: NÃO")]
    public int naoNotas;
    public int naoCansaco;
    public int naoSocializacao;
    public int naoEstresse;
}