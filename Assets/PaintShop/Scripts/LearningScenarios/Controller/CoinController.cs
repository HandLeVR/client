using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the spawn and the fade out of the three possible coins (green, golden and red).
/// </summary>
public class CoinController : Singleton<CoinController>
{
    // possible coins
    public Coin continueCoin;
    public Coin goldenContinueCoin;
    public Coin returnCoin;

    // position of the coins depending on the virtual instructor (coins are always below the virtual instructor)
    public Transform introductionPosition;
    public Transform paintPosition;

    // determines if a coin is currently selectable (they can not be selected shortly after selection)
    [HideInInspector] public bool coinSelectable;

    private List<Coin> _coins = new();
    private int _fadingCoins;
    private UnityAction _allCoinsFadedOut;

    private void Awake()
    {
        _coins = new List<Coin> { continueCoin, goldenContinueCoin, returnCoin };
    }

    private void Start()
    {
        coinSelectable = true;
    }

    /// <summary>
    /// Fades in or out the continue coin or the return coin in dependence of their current state. If a coin should be
    /// visible but is currently not visible it gets faded in. A coin that should not be visible but is currently
    /// visible gets faded out. 
    /// </summary>
    /// <param name="spawnContinueCoin">Should the continue coin be visible?</param>
    /// <param name="spawnReturnCoin">Should the return coin be visible?</param>
    /// <param name="showGoldenCoin">Determines whether the green continue coin gets replaced by a golden one.</param>
    public void FadeInOrOutCoins(bool spawnContinueCoin, bool spawnReturnCoin, bool showGoldenCoin = false)
    {
        VRSubTaskController subTaskController = LearningScenariosTaskController.Instance.currentSubTaskController;
        if (spawnContinueCoin)
        {
            if (goldenContinueCoin.gameObject.activeSelf && !showGoldenCoin)
                goldenContinueCoin.gameObject.SetActive(false);
            if (continueCoin.gameObject.activeSelf && showGoldenCoin)
                continueCoin.gameObject.SetActive(false);
        }

        FadeInOrOutCoin(showGoldenCoin ? goldenContinueCoin : continueCoin, subTaskController.ContinueCoinSelected,
            subTaskController.ContinueCoinFadedOut, spawnContinueCoin);
        FadeInOrOutCoin(returnCoin, subTaskController.ReturnCoinSelected, subTaskController.ReturnCoinFadedOut,
            spawnReturnCoin);
        Transform pos =
            VirtualInstructorController.Instance.currentPosition ==
            VirtualInstructorController.InstructorPosition.Introduction
                ? introductionPosition
                : paintPosition;
        transform.SetPositionAndRotation(pos.position, pos.rotation);
    }

    /// <summary>
    /// Fades out all coins and animates the initiator.
    /// </summary>
    public void FadeOutCoins(Coin initiator = null, UnityAction afterFadeOut = null)
    {
        _fadingCoins = 0;
        _allCoinsFadedOut = afterFadeOut;
        foreach (Coin coin in _coins.Where(coin => coin.gameObject.activeSelf))
        {
            if (initiator == null || !coin.Equals(initiator))
            {
                coin.FadeOut(afterFadeOut: () =>
                {
                    _fadingCoins--;
                    if (_fadingCoins <= 0)
                        _allCoinsFadedOut?.Invoke();
                });
                _fadingCoins++;
            }
        }
    }

    private void FadeInOrOutCoin(Coin coin, UnityAction afterSelection, UnityAction afterFadeOut, bool fadeIn)
    {
        if (fadeIn)
            coin.FadeIn(transform.position.y, afterSelection, afterFadeOut);
        else if (coin.gameObject.activeSelf)
            coin.FadeOut();
    }
}