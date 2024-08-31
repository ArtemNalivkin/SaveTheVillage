using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private int wheatCount = 0;
    private float wheatTimeLeft;
    [SerializeField] private float wheatTime = 5f;
    [SerializeField] private int wheatPerRound = 2;
    [SerializeField] private int wheatPerKnight = 2;
    [SerializeField] public TextMeshProUGUI wheatText;
    [SerializeField] public TextMeshProUGUI wheatPerRoundText;
    [SerializeField] public Image wheatTimerImage;

    private int villagersCount = 0;
    private bool isVillagerHiring = false;
    private float villagerHireTimeLeft;
    [SerializeField] private float villagerHireTime= 3.0f;
    [SerializeField] private int villagerPrice = 2;
    [SerializeField] private TextMeshProUGUI villagersText;
    [SerializeField] private Image villagerTimerImage;

    private int knightsCount = 0;
    private bool isKnightHiring = false;
    private float knightHireTimeLeft;
    [SerializeField] private float knightHireTime = 5;
    [SerializeField] private int knightPrice = 8;
    [SerializeField] private TextMeshProUGUI knightsText;
    [SerializeField] private Image knightTimerImage;

    private int currentWave = 0;
    private int wavesToOrcs = 0;
    private float waveAttackTimeLeft;
    [SerializeField] private float waveAttackTime = 10.0f;
    [SerializeField] private TextMeshProUGUI waveOrcsCountText;
    [SerializeField] private TextMeshProUGUI wavesToOrcsText;
    [SerializeField] private Image waveTimerImage;
    [SerializeField] private List<int> wavesArray = new List<int>();
    [SerializeField] private const int enemyIncreaseAfterLastWave = 5;

    [SerializeField] private Button hireVillagerButton;
    [SerializeField] private Button hireKnightButton;

    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private TextMeshProUGUI loseWavesText;

    [SerializeField] private AudioClip wheatClip;
    [SerializeField] private AudioClip hiredClip;
    [SerializeField] private AudioClip orcsClip;

    private bool isPaused = false;
    private bool isMuted = false;

    private AudioSource audioSource;

    private void Start()
    {
        InitParams();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateWheat();
        UpdateVillagers();
        UpdateKnights();
        UpdateWave();

        UpdateTimerImage(wheatTimerImage, wheatTimeLeft, wheatTime);
        UpdateTimerImage(knightTimerImage, knightHireTimeLeft, knightHireTime);
        UpdateTimerImage(villagerTimerImage, villagerHireTimeLeft, villagerHireTime);
        UpdateTimerImage(waveTimerImage, waveAttackTimeLeft, waveAttackTime);

        CheckWin();
    }

    private void InitParams()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        isPaused = false;

        wheatCount = 0;
        knightsCount = 0;
        villagersCount = 1;
        currentWave = 0;
        wheatTimeLeft = wheatTime;
        waveAttackTimeLeft = waveAttackTime;

        wheatText.text = "0";
        knightsText.text = "0";
        villagersText.text = "1";
        waveOrcsCountText.text = "1";

        isVillagerHiring = false;
        isKnightHiring = false;

        wheatText.text = wheatCount.ToString();
        wheatPerRoundText.text = GetWheatPerRound().ToString();
        villagersText.text = villagersCount.ToString();
        knightsText.text = knightsCount.ToString();
        waveOrcsCountText.text = wavesArray[currentWave].ToString();

        for (int i = 0; i != wavesArray.Count; ++i)
        {
            if (wavesArray[i] > 0)
            {
                wavesToOrcs = i;
                wavesToOrcsText.text = i.ToString();
                break;
            }
        }

        UpdateTimerImage(wheatTimerImage, wheatTimeLeft, wheatTime);
        UpdateTimerImage(knightTimerImage, knightHireTimeLeft, knightHireTime);
        UpdateTimerImage(villagerTimerImage, villagerHireTimeLeft, villagerHireTime);
        UpdateTimerImage(waveTimerImage, waveAttackTimeLeft, waveAttackTime);

        UpdateHireButtons();
    }

    private void UpdateWavesToOrcs()
    {
        if (wavesToOrcs > 0)
        {
            --wavesToOrcs;
            wavesToOrcsText.text = wavesToOrcs.ToString();
        }
    }


    private void UpdateWave()
    {
        waveAttackTimeLeft -= Time.deltaTime;
        if (waveAttackTimeLeft <= 0)
        {
            
            knightsCount -= wavesArray[currentWave];
            waveAttackTimeLeft = waveAttackTime;
            knightsText.text = knightsCount.ToString();
            wheatPerRoundText.text = GetWheatPerRound().ToString();


            if (knightsCount < 0)
            {
                //поражение
                Time.timeScale = 0;
                loseWavesText.text = currentWave.ToString();
                losePanel.SetActive(true);
            }
            else
            {
                if (currentWave < wavesArray.Count - 1)
                {
                    currentWave++;
                }
                else //если превысили предустановленное количество волн, прибавляем фиксрованное значение каждую следующую волну
                {
                    wavesArray[currentWave] += enemyIncreaseAfterLastWave;
                }

                UpdateWavesToOrcs();
                waveOrcsCountText.text = wavesArray[currentWave].ToString();

                audioSource.PlayOneShot(orcsClip);
            }
        }
    }

    private int GetWheatPerRound()
    {
        return Math.Max(villagersCount * wheatPerRound - knightsCount * wheatPerKnight, 0);
    }
    
    private void UpdateWheat()
    {
        wheatTimeLeft -= Time.deltaTime;

        if (wheatTimeLeft <= 0)
        {
            wheatCount += GetWheatPerRound();
            wheatTimeLeft = wheatTime;
            wheatText.text = wheatCount.ToString();
            UpdateHireButtons();

            audioSource.PlayOneShot(wheatClip);
        }
    }

    private void UpdateVillagers()
    {
        if (isVillagerHiring)
        {
            villagerHireTimeLeft -= Time.deltaTime;
            if (villagerHireTimeLeft <= 0)
            {
                isVillagerHiring = false;
                villagersCount++;
                villagersText.text = villagersCount.ToString();
                wheatPerRoundText.text = GetWheatPerRound().ToString();

                audioSource.PlayOneShot(hiredClip);
            }
        }
    }

    private void UpdateKnights()
    {
        if (isKnightHiring)
        {
            knightHireTimeLeft -= Time.deltaTime;
            if (knightHireTimeLeft <= 0)
            {
                isKnightHiring = false;
                knightsCount++;
                knightsText.text = knightsCount.ToString();
                wheatPerRoundText.text = GetWheatPerRound().ToString();

                audioSource.PlayOneShot(hiredClip);
            }
        }
    }

    private void CheckWin()
    {
        if(wheatCount >= 300 || villagersCount > 50)
        {
            Time.timeScale = 0;
            winPanel.SetActive(true);
        }
    }

    private void UpdateHireButtons()
    {
        hireVillagerButton.interactable = wheatCount >= villagerPrice;
        hireKnightButton.interactable = wheatCount >= knightPrice;
    }

    public void HireVillagerAction()
    {
        if(wheatCount >= villagerPrice && isVillagerHiring == false)
        {
            wheatCount -= villagerPrice;
            isVillagerHiring = true;
            villagerHireTimeLeft = villagerHireTime;
            wheatText.text = wheatCount.ToString();
            UpdateHireButtons();
        }
    }

    public void HireKnightAction()
    {
        if (wheatCount >= knightPrice && isKnightHiring == false) 
        {
            wheatCount -= knightPrice;
            isKnightHiring = true;
            knightHireTimeLeft = knightHireTime;
            wheatText.text = wheatCount.ToString();
            UpdateHireButtons();
        }
    }

    public void RestartAction()
    {
        Time.timeScale = 1;
        wheatTimeLeft = wheatTime;
        waveAttackTimeLeft = waveAttackTime;
        InitParams();
    }

    public void PauseAction()
    {
        if (isPaused)
        {
            Time.timeScale = 1;
            pausePanel.SetActive(false);
            isPaused = false;
        }
        else
        {
            Time.timeScale = 0;
            pausePanel.SetActive(true);
            isPaused = true;
        }
    }

    public void SoundAction()
    {
        if (isMuted)
        {
            audioSource.volume = 1.0f;
            isMuted = false;
        }
        else
        {
            audioSource.volume = 0.0f;
            isMuted = true;
        }
    }

    public void UpdateTimerImage(Image img, float currentTime, float maxTime)
    {
        img.fillAmount = currentTime / maxTime;
    }
}
