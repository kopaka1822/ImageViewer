# Statistics

The statistics tab can show very basic statistics for an image equation. By default it will display the **average**, **min** and **max** *Luminance*. 

*Luminance* is the radiant power weighted by a spectral sensitivity function that is characteristic of vision. 
The magnitude is proportional to physical power, but the spectral composition is related to the brightness sensitivity of human vision.
*Luminance* is computed in linear color space with: dot(RGB*A, (0.2125, 0.7154, 0.0721)).

 The following *components* can be selected instead of luminance:

* *Average* - equally weights each channel (good for general error comparison)
* *Luma* - video luma (sRGB)
* *Lightness* - gamma corrected luminance
* *Alpha* - image alpha channel (just for fun)
* *SSIM* - compares two images with the [SSIM](#SSIM) index

## MAE/MSE and more

Most of the popular error metrics can be computed by using an appropriate image equation in combination with the statistics tab. In this case `I0` is the original and `I1` the biased image:

|Error Metric|Image Equation  |Statistic| Full Name|
|------------|----------------|---------|----------|
|MAE         |`abs(I1 - I0)`  |*Average*|Mean Average Error|
|MSE         |`(I1-I0)^2`     |*Average*|Mean Squared Error|
|RMSE        |`(I1-I0)^2`     |*Root Average*|Root MSE|
|RMSRE       |`(I1/I0-1)^2`   |*Root Average*|Root Mean Squared Relative Error|

Rule of thumb:
* Choose *Luminance* for RGB weighted linear error
* Choose *Average* if RGB should be weighted equally
* Choose *SSIM* for percieved error

## SSIM

The [Structural Similarity (SSIM) index](https://www.cns.nyu.edu/pub/eero/wang03-reprint.pdf) is another method for predicting the percieved difference between two images. SSIM is based on visible structure differences instead of per-pixel absolute differences (like RMSE or MAE). 
It is computed from the luma grayscale image and ranges from -1 for inverse to 1 for identical images.