import cv2
import numpy as np
import os

image_path = 'Assets/Sprites/Chickscombined.png'
if not os.path.exists(image_path):
    # Try jpg if png doesn't exist
    image_path = 'Assets/Sprites/Chickscombined.jpg'
    if not os.path.exists(image_path):
        image_path = 'Assets/Sprites/Chickscombined.jpeg'

print(f"Reading image from: {image_path}")
img = cv2.imread(image_path, cv2.IMREAD_UNCHANGED)

if img is None:
    print("Error: Could not load image.")
    exit(1)

# Convert to BGRA if it doesn't have an alpha channel
if img.shape[2] == 3:
    img = cv2.cvtColor(img, cv2.COLOR_BGR2BGRA)

# Get the background color from top-left pixel
bg_color = img[0, 0].copy()
# OpenCV uses BGR
# Define tolerance
tolerance = 60

# Create mask of background
# Check distance from bg_color for all pixels
diff = np.abs(img[:, :, :3].astype(np.int32) - bg_color[:3].astype(np.int32))
bg_mask = np.all(diff < tolerance, axis=-1)

# Make background transparent
img[bg_mask] = [0, 0, 0, 0]

# Despill edges (where green > red/blue)
b, g, r, a = cv2.split(img)
max_rb = np.maximum(r, b)
despill_mask = (g > max_rb + 10) & (a > 0)
g[despill_mask] = max_rb[despill_mask]
a_float = a.astype(np.float32)
green_diff = g.astype(np.float32) - max_rb.astype(np.float32)
a_float[despill_mask] = np.clip(a_float[despill_mask] - green_diff[despill_mask] * 2, 0, 255)
img = cv2.merge((b, g, r, a_float.astype(np.uint8)))

# Find connected components in the foreground
fg_mask = (img[:, :, 3] > 0).astype(np.uint8) * 255

num_labels, labels, stats, centroids = cv2.connectedComponentsWithStats(fg_mask, connectivity=8)

names = ["Chick_1", "Chick_2", "Chick_3", "Nest", "Chick_4", "Nest_Extra"] # Added extra just in case
valid_components = []

for i in range(1, num_labels):
    x, y, w, h, area = stats[i]
    if area > 100: # ignore small noise
        valid_components.append((y, x, w, h))

# Sort top-to-bottom, then left-to-right
valid_components.sort(key=lambda item: (item[0]//100, item[1]))

print(f"Found {len(valid_components)} sprites.")

for idx, comp in enumerate(valid_components):
    y, x, w, h = comp
    pad = 5
    y1 = max(0, y - pad)
    x1 = max(0, x - pad)
    y2 = min(img.shape[0], y + h + pad)
    x2 = min(img.shape[1], x + w + pad)
    
    sprite = img[y1:y2, x1:x2]
    
    name = names[idx] if idx < len(names) else f"Extracted_{idx+1}"
    out_path = f'Assets/Sprites/{name}.png'
    cv2.imwrite(out_path, sprite)
    print(f"Saved {out_path}")
