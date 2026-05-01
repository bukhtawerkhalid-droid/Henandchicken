using UnityEngine;

public class ExitCheck : MonoBehaviour
{
    private GameFlowManager gameFlowManager;

    private void Awake()
    {
        gameFlowManager = FindFirstObjectByType<GameFlowManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            UIManager ui = FindFirstObjectByType<UIManager>();

            if (player != null && ui != null && !player.isGameOver)
            {
                if (player.chicks.Count >= player.targetChicks)
                {
                    player.isGameOver = true;
                    ui.ShowWin();
                }
                else
                {
                    ui.ShowFail();

                    if (gameFlowManager != null)
                    {
                        gameFlowManager.ShowLoseScreen();
                    }
                }
            }
        }
    }
}