using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class NetworkedHealthSystem : MonoBehaviourPunCallbacks, IPunObservable
{
    public int maxHealth = 100;
    private int currentHealth;

    public Canvas healthCanvas;
    public Text healthText;
    public Canvas gameOverCanvas;

    void Start()
    {
        currentHealth = maxHealth;

        // Only the owner of the GameObject (local player) should have control over its health
        if (photonView.IsMine)
        {
            healthCanvas.gameObject.SetActive(true);
            gameOverCanvas.gameObject.SetActive(false);
        }
        else
        {
            healthCanvas.gameObject.SetActive(false);
            gameOverCanvas.gameObject.SetActive(false);
        }

        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        healthText.text = "Health: " + currentHealth;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AKMBullet"))
        {
            TakeDamage(10);
        }
    }

    public void TakeDamage(int damage)
    {
        if (!photonView.IsMine)
            return;

        currentHealth -= damage;
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            // Player is dead
            Die();
        }
    }

    void Die()
    {
        if (!photonView.IsMine)
            return;

        photonView.RPC("GameOver", RpcTarget.AllBuffered);

        // Disable the player's controls, etc., if needed
        // ...

        // Leave the room and destroy the player's GameObject after 3 seconds
        Invoke("LeaveRoomAndDestroy", 3f);
    }

    [PunRPC]
    void GameOver()
    {
        // Show the game over canvas for all players
        gameOverCanvas.gameObject.SetActive(true);
    }

    void LeaveRoomAndDestroy()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Destroy(gameObject);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // Implement if needed for custom synchronization
    }
}