﻿namespace WaveOut {
    public class Const {
        public static readonly int SampleRate = 44100;
        public static readonly double DeltaTime = 1.0 / SampleRate;
        public static readonly double EnvelopeSpeed = 20.0;

        public static readonly double[] Level = new double[] {
            0.007874, 0.008180, 0.008498, 0.008828, 0.009171, 0.009528, 0.009898, 0.010283,
            0.010683, 0.011099, 0.011530, 0.011978, 0.012444, 0.012928, 0.013431, 0.013953,
            0.014495, 0.015059, 0.015644, 0.016253, 0.016885, 0.017541, 0.018223, 0.018932,
            0.019668, 0.020432, 0.021227, 0.022052, 0.022910, 0.023800, 0.024726, 0.025687,
            0.026686, 0.027723, 0.028801, 0.029921, 0.031084, 0.032293, 0.033549, 0.034853,
            0.036208, 0.037616, 0.039078, 0.040598, 0.042176, 0.043816, 0.045520, 0.047289,
            0.049128, 0.051038, 0.053023, 0.055084, 0.057226, 0.059451, 0.061762, 0.064164,
            0.066658, 0.069250, 0.071942, 0.074740, 0.077646, 0.080664, 0.083801, 0.087059,
            0.090444, 0.093960, 0.097613, 0.101409, 0.105351, 0.109447, 0.113703, 0.118124,
            0.122716, 0.127487, 0.132444, 0.137594, 0.142943, 0.148501, 0.154275, 0.160273,
            0.166504, 0.172978, 0.179703, 0.186690, 0.193949, 0.201490, 0.209324, 0.217462,
            0.225917, 0.234701, 0.243826, 0.253306, 0.263154, 0.273386, 0.284015, 0.295058,
            0.306530, 0.318447, 0.330829, 0.343691, 0.357054, 0.370936, 0.385358, 0.400341,
            0.415906, 0.432077, 0.448876, 0.466328, 0.484459, 0.503295, 0.522863, 0.543192,
            0.564311, 0.586252, 0.609045, 0.632725, 0.657325, 0.682882, 0.709433, 0.737016,
            0.765671, 0.795440, 0.826367, 0.858496, 0.891874, 0.926550, 0.962575, 1.000000
        };

        public static readonly double[] Amp = new double[] {
            0.000000, 0.000062, 0.000248, 0.000558, 0.000992, 0.001550, 0.002232, 0.003038,
            0.003968, 0.005022, 0.006200, 0.007502, 0.008928, 0.010478, 0.012152, 0.013950,
            0.015872, 0.017918, 0.020088, 0.022382, 0.024800, 0.027342, 0.030008, 0.032798,
            0.035712, 0.038750, 0.041912, 0.045198, 0.048608, 0.052142, 0.055800, 0.059582,
            0.063488, 0.067518, 0.071672, 0.075950, 0.080352, 0.084878, 0.089528, 0.094302,
            0.099200, 0.104222, 0.109368, 0.114638, 0.120032, 0.125550, 0.131192, 0.136958,
            0.142848, 0.148862, 0.155000, 0.161262, 0.167648, 0.174158, 0.180792, 0.187550,
            0.194432, 0.201438, 0.208568, 0.215822, 0.223200, 0.230702, 0.238328, 0.246078,
            0.253953, 0.261951, 0.270073, 0.278319, 0.286689, 0.295183, 0.303801, 0.312543,
            0.321409, 0.330399, 0.339513, 0.348751, 0.358113, 0.367599, 0.377209, 0.386943,
            0.396801, 0.406783, 0.416889, 0.427119, 0.437473, 0.447951, 0.458553, 0.469279,
            0.480129, 0.491103, 0.502201, 0.513423, 0.524769, 0.536239, 0.547833, 0.559551,
            0.571393, 0.583359, 0.595449, 0.607663, 0.620001, 0.632463, 0.645049, 0.657759,
            0.670593, 0.683551, 0.696633, 0.709839, 0.723169, 0.736623, 0.750202, 0.763904,
            0.777730, 0.791680, 0.805754, 0.819952, 0.834274, 0.848720, 0.863290, 0.877984,
            0.892802, 0.907744, 0.922810, 0.938000, 0.953314, 0.968752, 0.984314, 1.000000
        };

        public static readonly double[] FeedBack = new double[]
        {
            0.000000, 0.023256, 0.045802, 0.067669, 0.088889, 0.109489, 0.129496, 0.148936,
            0.167832, 0.186207, 0.204082, 0.221477, 0.238411, 0.254902, 0.270968, 0.286624,
            0.301887, 0.316770, 0.331288, 0.345455, 0.359281, 0.372781, 0.385965, 0.398844,
            0.411429, 0.423729, 0.435754, 0.447514, 0.459016, 0.470270, 0.481283, 0.492063,
            0.502618, 0.512953, 0.523077, 0.532995, 0.542714, 0.552239, 0.561576, 0.570732,
            0.579710, 0.588517, 0.597156, 0.605634, 0.613953, 0.622120, 0.630137, 0.638009,
            0.645740, 0.653333, 0.660793, 0.668122, 0.675325, 0.682403, 0.689362, 0.696203,
            0.702929, 0.709544, 0.716049, 0.722449, 0.728745, 0.734940, 0.741036, 0.747036,
            0.752941, 0.758755, 0.764479, 0.770115, 0.775665, 0.781132, 0.786517, 0.791822,
            0.797048, 0.802198, 0.807273, 0.812274, 0.817204, 0.822064, 0.826855, 0.831579,
            0.836237, 0.840830, 0.845361, 0.849829, 0.854237, 0.858586, 0.862876, 0.867110,
            0.871287, 0.875410, 0.879479, 0.883495, 0.887460, 0.891374, 0.895238, 0.899054,
            0.902821, 0.906542, 0.910217, 0.913846, 0.917431, 0.920973, 0.924471, 0.927928,
            0.931343, 0.934718, 0.938053, 0.941349, 0.944606, 0.947826, 0.951009, 0.954155,
            0.957265, 0.960340, 0.963380, 0.966387, 0.969359, 0.972299, 0.975207, 0.978082,
            0.980926, 0.983740, 0.986523, 0.989276, 0.992000, 0.994695, 0.997361, 1.000000
        };

        public static readonly double[] SemiTone = new double[] {
            1.000000, 1.059463, 1.122462, 1.189207, 1.259921, 1.334840, 1.414214, 1.498307,
            1.587401, 1.681793, 1.781797, 1.887749, 2.000000, 2.118926, 2.244924, 2.378414,
            2.519842, 2.669680, 2.828427, 2.996614, 3.174802, 3.363586, 3.563595, 3.775497,
            4.000000, 4.237852, 4.489848, 4.756828, 5.039684, 5.339359, 5.656854, 5.993228,
            6.349604, 6.727171, 7.127190, 7.550995, 8.000000, 8.475705, 8.979696, 9.513657,
            10.07936, 10.67871, 11.31370, 11.98645, 12.69920, 13.45434, 14.25437, 15.10198,
            16.00000, 16.95141, 17.95939, 19.02731, 20.15873, 21.35743, 22.62741, 23.97291,
            25.39841, 26.90868, 28.50875, 30.20397, 32.00000, 33.90281, 35.91878, 38.05462,
            40.31747, 42.71487, 45.25483, 47.94582, 50.79683, 53.81737, 57.01751, 60.40795,
            64.00000, 67.80563, 71.83757, 76.10925, 80.63494, 85.42975, 90.50966, 95.89165,
            101.5936, 107.6347, 114.0350, 120.8159, 128.0000, 135.6112, 143.6751, 152.2185,
            161.2698, 170.8595, 181.0193, 191.7833, 203.1873, 215.2694, 228.0700, 241.6318,
            256.0000, 271.2225, 287.3502, 304.4370, 322.5397, 341.7190, 362.0386, 383.5666,
            406.3746, 430.5389, 456.1401, 483.2636, 512.0000, 542.4451, 574.7005, 608.8740,
            645.0795, 683.4380, 724.0773, 767.1332, 812.7493, 861.0779, 912.2802, 966.5272,
            1024.000, 1084.890, 1149.401, 1217.748, 1290.159, 1366.876, 1448.154, 1534.266
        };

        public static readonly double[] PitchMSB = new double[] {
            1.00000000, 1.00090294, 1.00180670, 1.00271128, 1.00361667, 1.00452287, 1.00542990, 1.00633775,
            1.00724641, 1.00815590, 1.00906621, 1.00997733, 1.01088929, 1.01180206, 1.01271566, 1.01363008,
            1.01454533, 1.01546141, 1.01637831, 1.01729605, 1.01821461, 1.01913400, 1.02005422, 1.02097527,
            1.02189715, 1.02281986, 1.02374341, 1.02466779, 1.02559301, 1.02651906, 1.02744595, 1.02837367,
            1.02930224, 1.03023164, 1.03116188, 1.03209296, 1.03302488, 1.03395764, 1.03489125, 1.03582569,
            1.03676098, 1.03769712, 1.03863410, 1.03957193, 1.04051060, 1.04145012, 1.04239049, 1.04333171,
            1.04427378, 1.04521670, 1.04616047, 1.04710510, 1.04805057, 1.04899690, 1.04994409, 1.05089213,
            1.05184102, 1.05279077, 1.05374138, 1.05469285, 1.05564518, 1.05659837, 1.05755241, 1.05850732
        };

        public static readonly double[] PitchLSB = new double[] {
            1.00000000, 1.00000705, 1.00001410, 1.00002115, 1.00002820, 1.00003526, 1.00004231, 1.00004936,
            1.00005641, 1.00006346, 1.00007051, 1.00007756, 1.00008462, 1.00009167, 1.00009872, 1.00010577,
            1.00011282, 1.00011988, 1.00012693, 1.00013398, 1.00014103, 1.00014808, 1.00015514, 1.00016219,
            1.00016924, 1.00017629, 1.00018334, 1.00019040, 1.00019745, 1.00020450, 1.00021155, 1.00021861,
            1.00022566, 1.00023271, 1.00023976, 1.00024682, 1.00025387, 1.00026092, 1.00026798, 1.00027503,
            1.00028208, 1.00028914, 1.00029619, 1.00030324, 1.00031029, 1.00031735, 1.00032440, 1.00033145,
            1.00033851, 1.00034556, 1.00035262, 1.00035967, 1.00036672, 1.00037378, 1.00038083, 1.00038788,
            1.00039494, 1.00040199, 1.00040904, 1.00041610, 1.00042315, 1.00043021, 1.00043726, 1.00044432,
            1.00045137, 1.00045842, 1.00046548, 1.00047253, 1.00047959, 1.00048664, 1.00049370, 1.00050075,
            1.00050781, 1.00051486, 1.00052191, 1.00052897, 1.00053602, 1.00054308, 1.00055013, 1.00055719,
            1.00056424, 1.00057130, 1.00057835, 1.00058541, 1.00059246, 1.00059952, 1.00060657, 1.00061363,
            1.00062069, 1.00062774, 1.00063480, 1.00064185, 1.00064891, 1.00065596, 1.00066302, 1.00067007,
            1.00067713, 1.00068419, 1.00069124, 1.00069830, 1.00070535, 1.00071241, 1.00071947, 1.00072652,
            1.00073358, 1.00074064, 1.00074769, 1.00075475, 1.00076180, 1.00076886, 1.00077592, 1.00078297,
            1.00079003, 1.00079709, 1.00080414, 1.00081120, 1.00081826, 1.00082531, 1.00083237, 1.00083943,
            1.00084648, 1.00085354, 1.00086060, 1.00086766, 1.00087471, 1.00088177, 1.00088883, 1.00089589
        };

        public static readonly double[] Cos = new double[] {
            1.000000, 0.999981, 0.999828, 0.999522, 0.999063, 0.998451, 0.997687, 0.996770,
            0.995701, 0.994479, 0.993105, 0.991579, 0.989901, 0.988072, 0.986092, 0.983961,
            0.981680, 0.979248, 0.976666, 0.973935, 0.971056, 0.968027, 0.964851, 0.961526,
            0.958055, 0.954437, 0.950674, 0.946764, 0.942710, 0.938512, 0.934170, 0.929685,
            0.925058, 0.920290, 0.915381, 0.910331, 0.905143, 0.899816, 0.894351, 0.888750,
            0.883012, 0.877140, 0.871133, 0.864993, 0.858721, 0.852317, 0.845783, 0.839119,
            0.832328, 0.825408, 0.818363, 0.811192, 0.803898, 0.796480, 0.788941, 0.781280,
            0.773501, 0.765603, 0.757587, 0.749456, 0.741211, 0.732852, 0.724380, 0.715798,
            0.707107, 0.698307, 0.689401, 0.680389, 0.671273, 0.662054, 0.652734, 0.643314,
            0.633796, 0.624180, 0.614470, 0.604665, 0.594768, 0.584779, 0.574702, 0.564536,
            0.554284, 0.543947, 0.533527, 0.523026, 0.512444, 0.501784, 0.491047, 0.480236,
            0.469350, 0.458393, 0.447366, 0.436270, 0.425108, 0.413880, 0.402589, 0.391237,
            0.379825, 0.368355, 0.356828, 0.345247, 0.333613, 0.321927, 0.310193, 0.298411,
            0.286584, 0.274713, 0.262799, 0.250846, 0.238854, 0.226825, 0.214762, 0.202666,
            0.190539, 0.178383, 0.166200, 0.153991, 0.141758, 0.129504, 0.117230, 0.104938,
            0.092631, 0.080309, 0.067974, 0.055629, 0.043276, 0.030916, 0.018552, 0.006184
        };

        public static readonly double[] Sin = new double[] {
            0.000000, 0.006184, 0.018552, 0.030916, 0.043276, 0.055629, 0.067974, 0.080309,
            0.092631, 0.104938, 0.117230, 0.129504, 0.141758, 0.153991, 0.166200, 0.178383,
            0.190539, 0.202666, 0.214762, 0.226825, 0.238854, 0.250846, 0.262799, 0.274713,
            0.286584, 0.298411, 0.310193, 0.321927, 0.333613, 0.345247, 0.356828, 0.368355,
            0.379825, 0.391237, 0.402589, 0.413880, 0.425108, 0.436270, 0.447366, 0.458393,
            0.469350, 0.480236, 0.491047, 0.501784, 0.512444, 0.523026, 0.533527, 0.543947,
            0.554284, 0.564536, 0.574702, 0.584779, 0.594768, 0.604665, 0.614470, 0.624180,
            0.633796, 0.643314, 0.652734, 0.662054, 0.671273, 0.680389, 0.689401, 0.698307,
            0.707107, 0.715798, 0.724380, 0.732852, 0.741211, 0.749456, 0.757587, 0.765603,
            0.773501, 0.781280, 0.788941, 0.796480, 0.803898, 0.811192, 0.818363, 0.825408,
            0.832328, 0.839119, 0.845783, 0.852317, 0.858721, 0.864993, 0.871133, 0.877140,
            0.883012, 0.888750, 0.894351, 0.899816, 0.905143, 0.910331, 0.915381, 0.920290,
            0.925058, 0.929685, 0.934170, 0.938512, 0.942710, 0.946764, 0.950674, 0.954437,
            0.958055, 0.961526, 0.964851, 0.968027, 0.971056, 0.973935, 0.976666, 0.979248,
            0.981680, 0.983961, 0.986092, 0.988072, 0.989901, 0.991579, 0.993105, 0.994479,
            0.995701, 0.996770, 0.997687, 0.998451, 0.999063, 0.999522, 0.999828, 0.999981
        };
    }
}
