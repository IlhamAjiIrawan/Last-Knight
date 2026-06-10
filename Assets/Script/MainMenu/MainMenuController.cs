using UnityEngine;
using UnityEngine.SceneManagement; // Dibutuhkan untuk pindah scene

public class MainMenuController : MonoBehaviour
{
    // Fungsi untuk memulai game baru (Pindah ke Scene Game/Level 1)
    public void NewGame()
    {
        SceneManager.LoadScene("Map1Village");
    }

    // Fungsi untuk load game (Logika load disesuaikan dengan sistem save Anda)
    // public void LoadGame()
    // {
    //     Debug.Log("Load Game Diklik! Masukkan logika load data di sini.");
    // }

    // Fungsi untuk membuka menu setting
    // public void OpenSetting()
    // {
    //     Debug.Log("Setting Diklik! Masukkan logika membuka panel setting di sini.");
    // }

    // Fungsi untuk keluar dari game
    public void ExitGame()
    {
        SceneManager.LoadScene("ExitGame");
    }
}