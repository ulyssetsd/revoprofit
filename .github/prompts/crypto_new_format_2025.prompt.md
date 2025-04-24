the format of account statement for crypto got changed in 2025 in comparaison with the format from 2022

2022 csv format was
Type,Product,Started Date,Completed Date,Description,Amount,Currency,Fiat amount,Fiat amount (inc. fees),Fee,Base currency,State,Balance
EXCHANGE,Current,2021-08-06 18:38:11,2021-08-06 18:38:11,Exchanged to ETH,-408.43876,DOGE,-71.28,-70.21,1.06,EUR,COMPLETED,0
EXCHANGE,Current,2018-06-12 14:16:32,2018-06-12 14:16:32,Exchanged to BTC,0.01713112,BTC,100,100,0,EUR,COMPLETED,0.01713112
CARD_PAYMENT,Current,2018-07-19 15:52:15,2018-07-20 5:28:14,Hotel On Booking.com,-0.00893541,BTC,-56.53,-56.53,0,EUR,COMPLETED,0.00819571
EXCHANGE,Current,2018-08-19 20:43:55,2018-08-19 20:43:55,Exchanged to EUR,-0.00819571,BTC,-45.1,-45.1,0,EUR,COMPLETED,0
CARD_REFUND,Current,2018-08-21 10:49:04,2018-08-21 19:20:13,Refund from Hotel On Booking.com,0.00893541,BTC,50.01,50.01,0,EUR,COMPLETED,0.00893541
EXCHANGE,Current,2018-08-27 8:09:18,2018-08-27 8:09:18,Exchanged to EUR,-0.00879132,BTC,-50,-50,0,EUR,COMPLETED,0.00014409
EXCHANGE,Current,2019-03-05 9:37:56,2019-03-05 9:37:56,Exchanged to EUR,-0.0001421,BTC,-0.46,-0.46,0,EUR,COMPLETED,0.00000199
EXCHANGE,Current,2020-03-28 17:17:03,2020-03-28 17:17:03,Exchanged to BTC,0.0087862,BTC,50,50,0,EUR,COMPLETED,0.00878819
EXCHANGE,Current,2020-03-30 22:30:08,2020-03-30 22:30:08,Exchanged to EUR,-0.00338913,BTC,-20,-20,0,EUR,COMPLETED,0.00539906

2025 csv format is
Symbol,Type,Quantity,Price,Value,Fees,Date
BTC,Buy,0.01713112,"€5,837.33",€100.00,€0.00,"Jun 12, 2018, 4:16:32 PM"
BTC,Payment,0.00893541,"$7,252.05",$64.80,$0.00,"Jul 20, 2018, 7:28:14 AM"
BTC,Sell,0.00819571,"€5,504.07",€45.10,€0.00,"Aug 19, 2018, 10:43:55 PM"
BTC,Other,0.00893541,"$7,252.05",$64.80,$0.00,"Aug 21, 2018, 9:20:13 PM"
BTC,Sell,0.00879132,"€5,687.43",€50.00,€0.00,"Aug 27, 2018, 10:09:18 AM"
BTC,Sell,0.0001421,"€3,237.31",€0.46,€0.00,"Mar 5, 2019, 10:37:56 AM"
BTC,Buy,0.0087862,"€5,690.74",€50.00,€0.00,"Mar 28, 2020, 6:17:03 PM"
BTC,Sell,0.00338913,"€5,901.23",€20.00,€0.00,"Mar 31, 2020, 12:30:08 AM"
BTC,Buy,0.00862756,"€6,333.30",€54.65,€0.00,"Apr 13, 2020, 1:49:57 PM"
BTC,Sell,0.012578,"€7,950.39",€100.00,€0.00,"May 12, 2020, 8:17:27 AM"
BTC,Sell,0.00144862,"€8,116.32",€11.75,€0.00,"May 27, 2020, 3:38:44 PM"
BTC,Buy,0.00045547,"€21,954.91",€10.00,€0.25,"Dec 29, 2020, 2:50:58 PM"
ETH,Buy,0.0167597,€596.67,€10.00,€0.25,"Dec 29, 2020, 2:54:12 PM"

i would like you to update the code to use this new csv line format, keep the code structure as is, and dont change much other than the csv line converter mapper etc
i added a failing test that should be the root of all your iteration: RevolutServiceEndToEndTest and RevolutCsvServiceTest, make sure to not replace the old revolut format with the new one, take the extended code approach and split the concern to its own 2025 format domain so it does incrementation and not replacement
