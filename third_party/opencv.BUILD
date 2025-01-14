# Copyright (c) 2021 homuler
#
# Use of this source code is governed by an MIT-style
# license that can be found in the LICENSE file or at
# https://opensource.org/licenses/MIT.

load("@bazel_skylib//rules:common_settings.bzl", "string_flag")
load("@bazel_skylib//lib:selects.bzl", "selects")
load("@rules_foreign_cc//foreign_cc:defs.bzl", "cmake")

package(default_visibility = ["//visibility:public"])

filegroup(
    name = "all",
    srcs = glob(["**"]),
)

string_flag(
    name = "switch",
    build_setting_default = "local",
    values = [
        "local",
        "cmake",
    ],
)

config_setting(
    name = "cmake_static",
    flag_values = {
        ":switch": "cmake",
    },
)

config_setting(
    name = "local_build",
    flag_values = {
        ":switch": "local",
    },
)

config_setting(
    name = "opencv_ios_source_build",
    flag_values = {
        ":switch": "cmake",
    },
    values = {
        "apple_platform_type": "ios",
    },
)

selects.config_setting_group(
    name = "cmake_static_win",
    match_all = ["@bazel_tools//src/conditions:windows", ":cmake_static"],
)

config_setting(
    name = "dbg_build",
    values = {"compilation_mode": "dbg"},
)

selects.config_setting_group(
    name = "dbg_cmake_static_win",
    match_all = [":cmake_static_win", ":dbg_build"],
)

selects.config_setting_group(
    name = "local_build_win",
    match_all = ["@bazel_tools//src/conditions:windows", ":local_build"],
)

alias(
    name = "opencv",
    actual = select({
        ":cmake_static": ":opencv_cmake",
        ":opencv_ios_source_build": "@ios_opencv_source//:opencv",
        "//conditions:default": ":opencv_binary",
    }),
)

alias(
    name = "opencv_binary",
    actual = select({
        "@mediapipe//mediapipe:android_x86": "@android_opencv//:libopencv_x86",
        "@mediapipe//mediapipe:android_x86_64": "@android_opencv//:libopencv_x86_64",
        "@mediapipe//mediapipe:android_arm": "@android_opencv//:libopencv_armeabi-v7a",
        "@mediapipe//mediapipe:android_arm64": "@android_opencv//:libopencv_arm64-v8a",
        "@mediapipe//mediapipe:ios": "@ios_opencv//:opencv",
        "@mediapipe//mediapipe:macos_arm64": "@macos_arm64_opencv//:opencv",
        "@mediapipe//mediapipe:macos_x86_64": "@macos_opencv//:opencv",
        "@mediapipe//mediapipe:windows": "@windows_opencv//:opencv",
        "@mediapipe//mediapipe:emscripten": "@wasm_opencv//:opencv",
        "//conditions:default": "@linux_opencv//:opencv",
    }),
)

OPENCV_MODULES = [
    "calib3d",
    "features2d",
    "highgui",
    "optflow",
    "video",
    "videoio",
    "imgcodecs",
    "imgproc",
    "core",
]

OPENCV_3RDPARTY_LIBS = [
    "IlmImf",
    "libjpeg-turbo",
    "libopenjp2",
    "libpng",
    "libtiff",
    "zlib",
]

# ENABLE_NEON=ON
OPENCV_3RDPARTY_LIBS_M1 = OPENCV_3RDPARTY_LIBS + ["tegra_hal"]

COMMON_CACHE_ENTRIES = {
    # The module list is always sorted alphabetically so that we do not
    # cause a rebuild when changing the link order.
    "BUILD_LIST": ",".join(sorted(OPENCV_MODULES)),
    "BUILD_opencv_apps": "OFF",
    "BUILD_opencv_python": "OFF",
    "BUILD_opencv_world": "ON",
    "BUILD_opencv_optflow": "ON",
    "BUILD_EXAMPLES": "OFF",
    "BUILD_PERF_TESTS": "OFF",
    "BUILD_TESTS": "OFF",
    "BUILD_JPEG": "ON",
    "BUILD_OPENEXR": "ON",
    "BUILD_OPENJPEG": "ON",
    "BUILD_PNG": "ON",
    "BUILD_TIFF": "ON",
    "BUILD_ZLIB": "ON",
    "WITH_1394": "OFF",
    "WITH_FFMPEG": "OFF",
    "WITH_GSTREAMER": "OFF",
    "WITH_GTK": "OFF",
    # Some symbols in ippicv and ippiw cannot be resolved, and they are excluded currently in the first place.
    # https://github.com/opencv/opencv/pull/16505
    "WITH_IPP": "OFF",
    "WITH_ITT": "OFF",
    "WITH_JASPER": "OFF",
    "WITH_V4L": "OFF",
    "WITH_WEBP": "OFF",
    "CV_ENABLE_INTRINSICS": "ON",
    "WITH_EIGEN": "ON",
    "ENABLE_CCACHE": "OFF",
    # flags for static build
    "BUILD_SHARED_LIBS": "OFF",
    "OPENCV_SKIP_PYTHON_LOADER": "ON",
    "OPENCV_SKIP_VISIBILITY_HIDDEN": "ON",
    "OPENCV_EXTRA_MODULES_PATH": "$$EXT_BUILD_ROOT$$/external/opencv_contrib/modules",
}

CACHE_ENTRIES = COMMON_CACHE_ENTRIES | select({
    ":cmake_static_win": {
        "CMAKE_CXX_FLAGS": "/std:c++14",
        # required to link to .dll statically
        "BUILD_WITH_STATIC_CRT": "OFF",
        "WITH_LAPACK": "ON",
    },
    "@mediapipe//mediapipe:macos_arm64": {
        "CMAKE_SYSTEM_NAME": "Darwin",
        "CMAKE_SYSTEM_PROCESSOR": "arm64",
        "CMAKE_SYSTEM_ARCHITECTURES": "arm64",
        "CMAKE_OSX_ARCHITECTURES": "arm64",
    },
    "@mediapipe//mediapipe:macos_x86_64": {
        "CMAKE_SYSTEM_NAME": "Darwin",
        "CMAKE_SYSTEM_PROCESSOR": "x86_64",
        "CMAKE_SYSTEM_ARCHITECTURES": "x86_64",
        "CMAKE_OSX_ARCHITECTURES": "x86_64",
    },
    "//conditions:default": {},
}) | select ({
    "@bazel_tools//src/conditions:windows" : {},
    "//conditions:default": {
        # https://github.com/opencv/opencv/issues/19846
        "WITH_LAPACK": "OFF",
        "WITH_PTHREADS": "ON",
        "WITH_PTHREADS_PF": "ON",
    },
})

cmake(
    name = "opencv_cmake",
    data = [
        "@opencv_contrib//:all",
    ],
    build_args = [
        "--verbose",
        "--parallel",
    ] + select({
        "@bazel_tools//src/conditions:darwin": ["`sysctl -n hw.ncpu`"],
        "//conditions:default": ["`nproc`"],
    }),
    # Values to be passed as -Dkey=value on the CMake command line;
    # here are serving to provide some CMake script configuration options
    cache_entries = CACHE_ENTRIES,
    generate_args = select({
        "@bazel_tools//src/conditions:windows": [
            "-G \"Visual Studio 17 2022\"",
            "-A x64",
        ],
        "//conditions:default": [],
    }),
    lib_source = "@opencv//:all",
    out_include_dir = select({
        "@bazel_tools//src/conditions:windows": "include",
        "//conditions:default": "include/opencv4",
    }),
    out_lib_dir = select({
        "@bazel_tools//src/conditions:windows": "x64/vc17",
        "//conditions:default": ".", # need to include lib/ and share/OpenCV/3rdparty/lib when building static libs
    }),
    out_static_libs = select({
        ":dbg_cmake_static_win": ["staticlib/opencv_world4100d.lib"],
        ":cmake_static_win": ["staticlib/opencv_world4100.lib"],
        "//conditions:default": ["lib/libopencv_world.a"],
    }) + select({
        ":dbg_cmake_static_win": ["staticlib/%sd.lib" % lib for lib in OPENCV_3RDPARTY_LIBS],
        ":cmake_static_win": ["staticlib/%s.lib" % lib for lib in OPENCV_3RDPARTY_LIBS],
        "@cpuinfo//:macos_arm64": ["lib/opencv4/3rdparty/lib%s.a" % lib for lib in OPENCV_3RDPARTY_LIBS_M1],
        "//conditions:default": ["lib/opencv4/3rdparty/lib%s.a" % lib for lib in OPENCV_3RDPARTY_LIBS],
    }),
    out_shared_libs =  [],
    linkopts = select({
        "@bazel_tools//src/conditions:windows": [],
        "//conditions:default": [
            "-ldl",
            "-lm",
            "-lpthread",
        ],
    }) + select({
        "@bazel_tools//src/conditions:windows": [],
        "@bazel_tools//src/conditions:darwin": [],
        "//conditions:default": ["-lrt"],
    }),
)
