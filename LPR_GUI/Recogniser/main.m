
im = imread(fileLoc);
im = imresize(im, [480 NaN]);
im1=im;
imgray = rgb2gray(im);
threshold = graythresh(im);
imbin =im2bw(im, threshold);
im = edge(imgray, 'sobel');
im = imdilate(im, strel('rectangle', [4 4]));
im = imfill(im, 'holes');
im = imerode(im, strel('diamond',10));
Iprops=regionprops(im,'BoundingBox', 'Area', 'Image');
area = Iprops.Area;
count = numel(Iprops);
maxa= area;
boundingBox = Iprops.BoundingBox;
for i=1:count
   if maxa<Iprops(i).Area
       maxa=Iprops(i).Area;
       boundingBox=Iprops(i).BoundingBox;
   end
end    
im = imcrop(imbin, boundingBox);
im = imresize(im, [240 NaN]);
im = imopen(im, strel('rectangle', [4 4]));
im = bwareaopen(~im, 500);
[h, w] = size(im);
Iprops=regionprops(im,'BoundingBox','Area', 'Image');
count = numel(Iprops);
noPlate=[];
for i=1:count
   ow = length(Iprops(i).Image(1,:));
   oh = length(Iprops(i).Image(:,1));
   if ow<(h/2) & oh>(h/3)
       letter=readLetter(Iprops(i).Image);
       noPlate=[noPlate letter];
   end
end
disp(noPlate);