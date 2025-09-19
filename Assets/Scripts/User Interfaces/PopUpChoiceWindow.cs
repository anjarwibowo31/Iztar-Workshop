using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupChoiceWindow : MonoBehaviour
{
    public static PopupChoiceWindow Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject windowRoot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Default: hidden
        windowRoot.SetActive(false);

        // Cancel button hanya close
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(Close);
    }

    /// <summary>
    /// Tampilkan popup konfirmasi.
    /// </summary>
    /// <param name="message">Pesan yang ditampilkan.</param>
    /// <param name="onConfirm">Callback saat tombol Confirm ditekan.</param>
    public void Show(string message, Action onConfirm = null)
    {
        windowRoot.SetActive(true);
        descriptionText.text = message;

        // Reset confirm listener dulu
        confirmButton.onClick.RemoveAllListeners();

        // Tambah listener baru
        confirmButton.onClick.AddListener(() =>
        {
            onConfirm?.Invoke();
            Close();
        });
    }

    /// <summary>
    /// Tutup popup dan reset state.
    /// </summary>
    public void Close()
    {
        // Reset UI supaya aman
        descriptionText.text = string.Empty;
        confirmButton.onClick.RemoveAllListeners();

        windowRoot.SetActive(false);
    }
}
