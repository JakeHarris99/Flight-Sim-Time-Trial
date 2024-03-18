using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Manager : MonoBehaviour
{
    //Allows labels to be assigned
    [SerializeField]
    private Text timerLabel;
    [SerializeField]
    private Text scoreLabel;
    [SerializeField]
    private Text healthLabel;
    [SerializeField]
    private Text livesLabel;
    [SerializeField]
    private Text powerupLabel;

    //sets up data variables
    private int score;
    private float time;
    private float health;
    private float lives;
    private float powerup;
    private float lastDamageTaken;
    private float lastBuffTime;
    private float buffDuration;
    private float buffTimer;

    //Initialises data values
    private void Start()
    {
        health = 100;
        lives = 1;
        score = 0;
        lastDamageTaken = Time.time;
        powerup = 0;
        lastBuffTime = Time.time;
    }

    void Update()
    {
        //Formats time into a timer style and applies to 
        time += Time.deltaTime;
        int minutes = (int)time / 60;
        int seconds = (int)time % 60;
        int fraction = (int)(time * 100) % 100;
        timerLabel.text = "Time: " + minutes.ToString() + ":" + seconds.ToString("00") + ":" + fraction.ToString("00");

        //Loads menu on escape press
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }

        //Creates a countdown timer for buff
        buffTimer = (lastBuffTime - Time.time) + buffDuration;
        setPowerupText();

        //Cheat button to allow completion after 1 collectable
        if(Input.GetKeyDown(KeyCode.P))
        {
            score = 8;
        }
    }

    //Adds 1 score and loads menu when score reaches 9
    public void addScore()
    {
        if(score == 8)
        {
            SceneManager.LoadScene(0);
        }
        score += 1;
        scoreLabel.text = "Score: " + score;
    }

    //Remove 50 health and call remove life when reaches 0
    public void takeDamage()
    {
        if (Time.time - lastDamageTaken > 1)
        {
            if (health == 100)
            {
                health -= 50;
                healthLabel.text = "Health: 50";
                lastDamageTaken = Time.time;
            }
            else
            {
                health = 100;
                healthLabel.text = "Health: 100";
                loseLife();
                lastDamageTaken = Time.time;
            }
        }
    }

    //Removes a life and calls reset plane, loads menu when lives reach 0
    private void loseLife()
    {
        if(lives == 1)
        {
            SceneManager.LoadScene(0);
        }
        lives -= 1;
        livesLabel.text = "Lives: " + lives;
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlaneController>().resetPlane();
    }

    //Sets powerup text to the current powerup with timer formatted to 2 d.p
    private void setPowerupText()
    {
        if(buffTimer < 0)
        {
            powerup = 0;
            powerupLabel.text = "Powerup: None (0)";
        }
        if (powerup == 1)
        {
            powerupLabel.text = "Powerup: Speed (" + Math.Round(buffTimer,2) + ")";
        }
        if (powerup == 2)
        {
            powerupLabel.text = "Powerup: Inverted (" + Math.Round(buffTimer, 2) + ")";
        }
        if (powerup == 3)
        {
            powerupLabel.text = "Powerup: Mobility (" + Math.Round(buffTimer, 2) + ")";
        }
    }

    //Assigns a random powerup if player doesnt have one
    public void givePowerup()
    {
        if(powerup == 0)
        {
            powerup = UnityEngine.Random.Range(1, 4);
            if (powerup == 1)
            {
                powerupLabel.text = "Powerup: Speed";
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlaneController>().speedBuff();
                lastBuffTime = Time.time;
                buffDuration = 5;
            }
            if (powerup == 2)
            {
                powerupLabel.text = "Powerup: Inverted";
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlaneController>().invertedBuff();
                lastBuffTime = Time.time;
                buffDuration = 5;
            }
            if (powerup == 3)
            {
                powerupLabel.text = "Powerup: Mobility";
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlaneController>().mobilityBuff();
                lastBuffTime = Time.time;
                buffDuration = 5;
            }
        }
    }
}
