#!/bin/sh

DEST="/usr/local/DiffScribe"

case "$SHELL" in
    */zsh)   PROFILE="$HOME/.zprofile"      ;; # zsh
    */bash)  PROFILE="$HOME/.bash_profile"  ;; # bash
    */ksh)   PROFILE="$HOME/.profile"       ;; # ksh / sh
    */fish)  PROFILE="$HOME/.config/fish/config.fish" ;; # fish shell
    *)       PROFILE="$HOME/.profile"       ;; # fallback
esac

sudo rm -rf "$DEST"
echo "Removed $DEST and its contents."

if grep -qs "$DEST" "$PROFILE"; then
    sed -i.bak '/DiffScribe/d' "$PROFILE"
    echo "$DEST removed from PATH. Restart your shell session."
fi