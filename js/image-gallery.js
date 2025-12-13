// Gallery lightbox - initialized once per page
(function() {
if (window.galleryLightboxInitialized) return;
window.galleryLightboxInitialized = true;

// Wait for DOM to be ready
const init = function() {
// Create lightbox modal if it doesn't exist
if (!document.getElementById('gallery-lightbox')) {
const lightboxHTML = '<div id="gallery-lightbox" class="gallery-lightbox" style="display: none;">' +
'<div class="lightbox-content">' +
'<button class="lightbox-close" aria-label="Close">&times;</button>' +
'<button class="lightbox-prev" aria-label="Previous">&#10094;</button>' +
'<button class="lightbox-next" aria-label="Next">&#10095;</button>' +
'<img src="" alt="" class="lightbox-image">' +
'<div class="lightbox-caption"></div>' +
'</div></div>';
document.body.insertAdjacentHTML('beforeend', lightboxHTML);
}

const lightbox = document.getElementById('gallery-lightbox');
const lightboxImg = lightbox.querySelector('.lightbox-image');
const lightboxCaption = lightbox.querySelector('.lightbox-caption');
const closeBtn = lightbox.querySelector('.lightbox-close');
const prevBtn = lightbox.querySelector('.lightbox-prev');
const nextBtn = lightbox.querySelector('.lightbox-next');

let currentImages = [];
let currentIndex = 0;

function openLightbox(images, index) {
currentImages = images;
currentIndex = index;
showImage(currentIndex);
lightbox.style.display = 'flex';
document.body.style.overflow = 'hidden';
}

function closeLightbox() {
lightbox.style.display = 'none';
document.body.style.overflow = '';
}

function showImage(index) {
if (currentImages.length === 0) return;
currentIndex = (index + currentImages.length) % currentImages.length;
const img = currentImages[currentIndex];
lightboxImg.src = img.dataset.full;
lightboxImg.alt = img.alt;
lightboxCaption.textContent = img.dataset.caption || img.alt || '';
prevBtn.style.display = currentImages.length > 1 ? 'block' : 'none';
nextBtn.style.display = currentImages.length > 1 ? 'block' : 'none';
}

function nextImage() {
showImage(currentIndex + 1);
}

function prevImage() {
showImage(currentIndex - 1);
}

closeBtn.addEventListener('click', closeLightbox);
prevBtn.addEventListener('click', prevImage);
nextBtn.addEventListener('click', nextImage);

lightbox.addEventListener('click', function(e) {
if (e.target === lightbox) {
closeLightbox();
}
});

document.addEventListener('keydown', function(e) {
if (lightbox.style.display !== 'flex') return;
if (e.key === 'Escape') closeLightbox();
if (e.key === 'ArrowRight') nextImage();
if (e.key === 'ArrowLeft') prevImage();
});

// Initialize all galleries on the page
document.querySelectorAll('.image-gallery').forEach(gallery => {
const thumbnails = Array.from(gallery.querySelectorAll('.gallery-thumbnail'));
thumbnails.forEach((thumb, index) => {
thumb.style.cursor = 'pointer';
thumb.addEventListener('click', () => openLightbox(thumbnails, index));
});
});
};

if (document.readyState === 'loading') {
document.addEventListener('DOMContentLoaded', init);
} else {
init();
}
})();
