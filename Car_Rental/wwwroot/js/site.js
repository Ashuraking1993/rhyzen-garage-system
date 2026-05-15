// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {

    const hero = document.querySelector(".hero-section");

    if (!hero) return;

    const images = [
        "/Images/hero1.png",
        "/Images/hero2.png",
        "/Images/hero3.png",
        "/Images/hero4.png",
        "/Images/hero5.png"
    ];

    let current = 0;

    // Set first image
    hero.style.backgroundImage = `url('${images[current]}')`;

    setInterval(() => {
        current = (current + 1) % images.length;
        hero.style.backgroundImage = `url('${images[current]}')`;
    }, 4000);

});