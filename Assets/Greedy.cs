using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Greedy : MonoBehaviour
{

    private List<Coin> _coins;

    [SerializeField][Range(0, 1)]
    private float epsilonStart;
    [SerializeField][Range(0, 1)]
    private float epsilonEnd;
    [SerializeField]
    private int maxEpoch;
    [SerializeField]
    private int target = 1000;

    private int currentIndex = 0;
        
    private void Start()
    {

        _coins = new List<Coin>()
        {
            new Coin { count = 0, chance = 0.25f, success = 0},
            new Coin { count = 0, chance = 0.5f, success = 0 },
            new Coin { count = 0, chance = 0.75f, success = 0 }
        };

        StartCoroutine(Predict());

    }


    private IEnumerator Predict()
    {
        int money = 0;
        
        int iteration = 0;
        
        while(money < target && money > -target)
        {

            iteration++;
            if (iteration % 50 == 0)
            {
                yield return null;
            }
            
            if (Random.Range(0f, 1f) <= GetEpsilon(iteration))
            {
                currentIndex = Random.Range(0, _coins.Count);
            }
            else
            {
                currentIndex = GetBestCoinIndex();
            }
            _coins[currentIndex].AddCount();
            if (Random.Range(0f, 1f) <= _coins[currentIndex].chance)
            {
                _coins[currentIndex].AddSuccess();
                money+=2;
            }
            money--;

        }
        
        
        foreach (var coin in _coins)
        {
            Debug.Log($"Coin: Chance: {coin.chance} | Stat: {coin.GetStat()}");
        }
    }

    private int GetBestCoinIndex()
    {
        int index = 0;
        float best = 0;
        foreach (var coin in _coins)
        {
            float coinStat = coin.GetStat();

            if(coinStat > best)
            {
                best = coinStat;
                index = _coins.IndexOf(coin);
            }
        }

        return index;
    }

    private float GetEpsilon(int epochNumber)
    {
        float r = Mathf.Max((float)(maxEpoch - epochNumber) / maxEpoch, 0);
        return (epsilonStart - epsilonEnd) * r + epsilonEnd;
    }
}

public class Coin
{

    public int count;
    public float chance;
    public int success;


    public void AddCount()
    {
        count++;
    }
    public void AddSuccess()
    {
        success++;
    }

    public float GetStat()
    {
        return (float)success / count;
    }
}