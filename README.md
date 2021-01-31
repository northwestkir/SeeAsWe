# SeeAsWe
A CSV parser made to be fast
# Benchmarks
It's designed to be 5-10 times faster then reading with csvhelper

|            Method |           data |            Mean |         Error |        StdDev |
|------------------ |--------------- |----------------:|--------------:|--------------:|
|      CsvHelperRun | Byte[27000035] | 1,534,098.50 us | 21,172.864 us | 18,769.193 us |
| SeeAsWeeParcerRun | Byte[27000035] |   285,909.06 us |  3,677.991 us |  3,440.395 us |
|      CsvHelperRun |  Byte[2700035] |   155,996.71 us |  1,956.410 us |  1,830.027 us |
| SeeAsWeeParcerRun |  Byte[2700035] |    31,385.76 us |    369.283 us |    345.428 us |
|      CsvHelperRun |   Byte[270035] |    16,185.55 us |    238.072 us |    222.692 us |
| SeeAsWeeParcerRun |   Byte[270035] |     2,874.52 us |     15.335 us |     13.594 us |
|      CsvHelperRun |    Byte[27035] |     2,331.71 us |      4.969 us |      4.149 us |
| SeeAsWeeParcerRun |    Byte[27035] |       300.22 us |      3.304 us |      3.090 us |
|      CsvHelperRun |     Byte[2735] |       962.58 us |     15.143 us |     14.165 us |
| SeeAsWeeParcerRun |     Byte[2735] |        31.89 us |      0.361 us |      0.338 us |
