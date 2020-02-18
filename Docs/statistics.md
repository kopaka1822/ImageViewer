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
|RMSRE (alt.)|`(2*(I1 - I0)/(I1 + I0))^2`|*Root Average*|Alternative version of RMSRE|

Rule of thumb:

* Choose *Luminance* for RGB weighted linear error
* Choose *Average* if RGB should be weighted equally
* Choose [*SSIM*](#SSIM) for perceived error

### PSNR

Peak signal-to-noise ratio (PSNR) is the ratio between the maximum possible power of a signal and the power of corrupting noise that affects the fidelity of its representation. PSNR is defined as:
`PSNR = 20*log10(MAX_I0) - 10*log10(MSE)`

Three steps are required to determine the PSNR:

1. set `I0` as equation and copy the *Max* value
2. set `(I1-I0)^2` as equation and copy the *Average* value
3. use first value as `MAX_I0` and second value as `MSE`

## SSIM

The [Structural Similarity (SSIM) index](https://www.cns.nyu.edu/pub/eero/wang03-reprint.pdf) is another method for predicting the perceived difference between two images. SSIM is based on visible structure differences instead of per-pixel absolute differences (like RMSE or MAE). It is computed with the luma grayscale (sRGB space).

|SSIM|Interpretation|
|----|--------------|
|1   |Images are identical|
|0   |Images have no relation|
|-1  |Images are inversed|
