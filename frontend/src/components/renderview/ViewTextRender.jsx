import React, { memo, useRef, useEffect } from "react";
import { Box } from "zmp-ui";
import { openWebview } from "zmp-sdk";

const ViewTextRender = ({ dataText, style }) => {
  const contentRef = useRef(null);

  useEffect(() => {
    const handleLinkClick = (event) => {
      event.preventDefault();
      const link = event.target.href;
      if (link) {
        webView(link);
      }
    };
    const contentElement = contentRef.current;
    if (contentElement) {
      const links = contentElement.querySelectorAll("a");
      links.forEach((link) => {
        link.addEventListener("click", handleLinkClick);
      });
    }

    return () => {
      if (contentElement) {
        const links = contentElement.querySelectorAll("a");
        links.forEach((link) => {
          link.removeEventListener("click", handleLinkClick);
        });
      }
    };
  }, [dataText]);

  const webView = async (link) => {
    try {
      await openWebview({
        url: link,
        config: {
          style: "bottomSheet",
          leftButton: "back",
        },
      });
    } catch (error) {
      console.log(error);
    }
  };

  return (
    <Box ref={contentRef} style={style}>
      <style>{`
        #view-text-render p {
          margin-bottom: 0 !important;
        }
      `}</style>

      <div
        id="view-text-render"
        dangerouslySetInnerHTML={{ __html: dataText }}
      />
    </Box>
  );
};

export default memo(ViewTextRender);
