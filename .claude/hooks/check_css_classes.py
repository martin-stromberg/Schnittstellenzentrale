#!/usr/bin/env python3
"""
PostToolUse-Hook: Prüft ob alle sz-*-CSS-Klassen in geänderten .razor-Dateien
in app.css oder einer .razor.css-Datei definiert sind.
"""
import sys
import json
import re
import os
import glob


def main():
    try:
        data = json.load(sys.stdin)
    except Exception:
        return

    file_path = data.get('tool_input', {}).get('file_path', '')

    # Nur .razor-Dateien prüfen (keine .razor.css)
    if not file_path.endswith('.razor'):
        return
    if file_path.endswith('.razor.css'):
        return

    # Nur Dateien innerhalb des Schnittstellenzentrale-Projekts
    normalized = file_path.replace('\\', '/')
    if '/Components/' not in normalized and '/Pages/' not in normalized:
        return

    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
    except Exception:
        return

    # sz-*-Klassen aus class="..."-Attributen extrahieren
    sz_classes = set()
    for match in re.finditer(r'class="([^"]*)"', content):
        for cls in match.group(1).split():
            if re.match(r'^sz-[a-z]', cls):
                sz_classes.add(cls)

    if not sz_classes:
        return

    # Projektwurzel über Verzeichnisbaum ermitteln
    project_root = find_project_root(file_path)
    if not project_root:
        return

    css_content = load_all_css(project_root)
    if not css_content:
        return

    missing = []
    for cls in sorted(sz_classes):
        # Klasse gilt als definiert wenn sie als CSS-Selektor vorkommt
        if not re.search(r'\.' + re.escape(cls) + r'[\s{:,\[\.]', css_content):
            missing.append(cls)

    if missing:
        file_name = os.path.basename(file_path)
        lines = [f"  • .{cls}" for cls in missing]
        message = (
            f"⚠️  Fehlende CSS-Definitionen in {file_name}:\n"
            + "\n".join(lines)
            + "\n→ Bitte in app.css oder einer .razor.css-Datei ergänzen (siehe CLAUDE.md)."
        )
        print(json.dumps({
            "hookSpecificOutput": {
                "hookEventName": "PostToolUse",
                "additionalContext": message
            }
        }))


def find_project_root(start_path: str) -> str | None:
    current = os.path.dirname(os.path.abspath(start_path))
    while current and current != os.path.dirname(current):
        app_css = os.path.join(current, 'src', 'Schnittstellenzentrale', 'wwwroot', 'app.css')
        if os.path.exists(app_css):
            return current
        current = os.path.dirname(current)
    return None


def load_all_css(project_root: str) -> str:
    parts = []

    app_css = os.path.join(project_root, 'src', 'Schnittstellenzentrale', 'wwwroot', 'app.css')
    if os.path.exists(app_css):
        with open(app_css, 'r', encoding='utf-8') as f:
            parts.append(f.read())

    pattern = os.path.join(
        project_root, 'src', 'Schnittstellenzentrale', 'Components', '**', '*.razor.css'
    )
    for css_file in glob.glob(pattern, recursive=True):
        try:
            with open(css_file, 'r', encoding='utf-8') as f:
                parts.append(f.read())
        except Exception:
            pass

    return '\n'.join(parts)


if __name__ == '__main__':
    main()
