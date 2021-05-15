# Imagely

Simple image editing software app built using c# and Universal Windows Platform (UWP). I built this around the end of 2019 - beginning of 2020.
My inspiration for this project was to create some computer generated art along with some other image manipulation aspects for fun.
This was a great learning experience at the time that also allowed to me create an app that me and my friends use for fun from time to time.
Take a look at what the app can do below.

# Image Triangulation

My image triangulation only accepts png images as they tend to be higher quality, but it does not work well with very large images.
Also, selecting 1000-6000 points tends to produce the best results.

Here is a few examples of the image triangulation.

<p align="center">
  <img width="476" height="721" src="https://user-images.githubusercontent.com/56607702/118341490-5ae2e480-b4ed-11eb-8018-53527439b3e7.png">
</p>

<p align="center">
  <img width="1059" height="707" src="https://user-images.githubusercontent.com/56607702/118341805-87e3c700-b4ee-11eb-968f-644c5d66c8dc.png">
</p>

### How It Works

* Get user input image
* Convert to grayscale
* Appy edge detection
* Apply [delaunay triangulation](https://en.wikipedia.org/wiki/Delaunay_triangulation)
* Place Triangles on canvas using average color of the area it is placed in

# Edge Detection

The edge detection used in this app is the [sobel operator](https://en.wikipedia.org/wiki/Sobel_operator).

Here is an example of the edge detection.

<p align="center">
  <img width="476" height="721" src="https://user-images.githubusercontent.com/56607702/118342427-55879900-b4f1-11eb-897d-b4a2418e512c.png">
</p>

# Abstract Triangulation

This works exactly as the image triangulation, but places the triangle points in random locations. This produces results similar to a [voronoi diagram](https://en.wikipedia.org/wiki/Voronoi_diagram) by using the [Bowyer-Watson algorithm](https://en.wikipedia.org/wiki/Bowyer%E2%80%93Watson_algorithm).
There is an option to select colors, or use random ones.

Here is an example of the abstraction triangulation.

<p align="center">
  <img width="1700" height="550" src="https://user-images.githubusercontent.com/56607702/118342392-2c670880-b4f1-11eb-94b7-c3c3dd61e42d.png">
</p>

# Abstract Circle Packing

This is a very simple implementation of [circle packing](https://en.wikipedia.org/wiki/Circle_packing). I wish to return to implement a better algorithm for this. There is an option to select colors, or use random ones.

Here is an example of the abstract circle packing.

<p align="center">
  <img width="1700" height="550" src="https://user-images.githubusercontent.com/56607702/118342545-bca54d80-b4f1-11eb-8219-6df1028cee2d.png">
</p>

# Image Circle Packing

I set everything up in order to do the image [circle packing](https://en.wikipedia.org/wiki/Circle_packing), but never got around to implementing the algorithm. I wish to return to implement this as I am still interested in creating computer generated art.

# Steganography

[Steganography](https://en.wikipedia.org/wiki/Steganography) is concealing a hidden message inside of an object. My implementation of this allows you to hide images or text inside of another image using bit manipulation.
When a small bits per color channel option is selected, it is not noticeable that there is a hidden message. It then allows you to extract the embedded message from the image back to its original form. There is also an option to encrypt or decrypt the messages, but it is very basic encryption.
