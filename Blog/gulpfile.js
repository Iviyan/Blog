"use strict";

var gulp = require("gulp"),
    rimraf = require("rimraf");/*,
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify");*/

const nodeRoot = './node_modules/';
const targetPath = './wwwroot/lib/';

gulp.task('clean', function (cb) {
    return rimraf(targetPath + '/**/*', cb);
});

gulp.task('copy_bootstrap', function () {
    gulp.src(nodeRoot + "bootstrap/dist/js/bootstrap.bundle.min.js").pipe(gulp.dest(targetPath + "/bootstrap/dist/js"));
    gulp.src(nodeRoot + "bootstrap/dist/css/bootstrap.min.css").pipe(gulp.dest(targetPath + "/bootstrap/dist/css"));
});

gulp.task('copy_ckeditor', function () {
    
    gulp.src(nodeRoot + "@ckeditor/ckeditor5-build-classic/build/ckeditor.js").pipe(gulp.dest(targetPath + "ckeditor/ckeditor5-build-classic/build"));
    gulp.src(nodeRoot + "@ckeditor/ckeditor5-build-classic/build/ckeditor.js.map").pipe(gulp.dest(targetPath + "ckeditor/ckeditor5-build-classic/build"));
    gulp.src(nodeRoot + "@ckeditor/ckeditor5-build-classic/build/translations/ru.js").pipe(gulp.dest(targetPath + "/ckeditor/ckeditor5-build-classic/build/translations/"));

    gulp.src(nodeRoot + "@ckeditor/ckeditor5-vue/dist/ckeditor.js").pipe(gulp.dest(targetPath + "/ckeditor/ckeditor5-vue/dist"));
    //gulp.src(nodeRoot + "@ckeditor/ckeditor5-upload/src/adapters/*").pipe(gulp.dest(targetPath + "/ckeditor/ckeditor5-upload/src/adapters"));
});

gulp.task('copy_copperjs', function () {
    gulp.src(nodeRoot + "cropperjs/dist/cropper.min.css").pipe(gulp.dest(targetPath + "cropperjs/dist"));
    gulp.src(nodeRoot + "cropperjs/dist/cropper.min.js").pipe(gulp.dest(targetPath + "cropperjs/dist"));
});

gulp.task('copy_vue-textarea-autosize', function () {
    return gulp.src(nodeRoot + "vue-textarea-autosize/dist/*").pipe(gulp.dest(targetPath + "vue-textarea-autosize/dist"));
});

gulp.task("copy_all", gulp.series(["copy_bootstrap", "copy_ckeditor", "copy_copperjs", "copy_vue-textarea-autosize"]));
gulp.task("clean_copy", gulp.series(["clean", "copy_all"]));