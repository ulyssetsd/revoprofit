please implement correctly the transformation of revoluttransaction2025 into crypto transaction make sure that for every type it make sense and eclude the one that doesnt make sens

Buy -> CryptoTransactionType.Buy
Sell -> CryptoTransactionType.Sell
Stake/Unstake -> Similar to Exchange as they convert between staked and unstaked versions
StakingReward/LearnReward -> CryptoTransactionType.Buy since you're receiving crypto, except its a free crypto so not sure how to know what is the price paid in this case
send/receive -> equivalent to an Exchange from one crypto (send) to another (receive), be careful if there is only a send maybe thats means something else like a transfer to another wallet (in this way im not sure how to interpret this)
Payment -> Should be considered as paying for something therefore a sell of the crypto asset right ?
