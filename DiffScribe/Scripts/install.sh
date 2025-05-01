#!/bin/sh

SRC="$(pwd)"
DEST="/usr/local/DiffScribe"

sudo mkdir -p "$DEST"
sudo cp -r "$SRC"/* "$DEST/"

case "$SHELL" in
    */zsh)   PROFILE="$HOME/.zprofile"      ;; # zsh
    */bash)  PROFILE="$HOME/.bash_profile"  ;; # bash
    */ksh)   PROFILE="$HOME/.profile"       ;; # ksh / sh
    */fish)  PROFILE="$HOME/.config/fish/config.fish" ;; # fish shell
    *)       PROFILE="$HOME/.profile"       ;; # fallback
esac

if ! grep -qs "$DEST" "$PROFILE"; then
    printf "\nexport PATH=\"\$PATH:$DEST\"" >> "$PROFILE"
    echo "Added $DEST to PATH. Restart your session."
fi