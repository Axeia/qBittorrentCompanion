from engines import limetorrents
from novaprinter import prettyPrinter

def printer(result):
    print(f"{result['name']} | {result['seeds']} seeds | {result['size']} | {result['desc_link']}")

# Patch novaprinter
import novaprinter
novaprinter.prettyPrinter = printer

# Run search
engine = limetorrents.limetorrents()
engine.search("frieren", "all")