(function(){
  async function fetchComments(visualizationId){
    const res = await fetch(`/api/comments?visualizationId=${visualizationId}`);
    if(!res.ok) return [];
    return await res.json();
  }

  function renderComments(container, comments){
    container.innerHTML = '';
    if(!comments.length){
      container.innerHTML = '<p class="muted">No comments yet — be the first to comment.</p>';
      return;
    }
    const list = document.createElement('div');
    list.className = 'comment-list';
    comments.forEach(c => {
      const el = document.createElement('div');
      el.className = 'comment-item card mb-2';
      el.innerHTML = `
        <div class="card-body">
          <div class="d-flex justify-content-between align-items-start mb-2">
            <strong>${escapeHtml(c.author)}</strong>
            <small class="muted">${new Date(c.createdAt).toLocaleString()}</small>
          </div>
          <div>${escapeHtml(c.content)}</div>
        </div>`;
      list.appendChild(el);
    });
    container.appendChild(list);
  }

  function escapeHtml(s){
    if(!s) return '';
    return s.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
  }

  async function postComment(dto){
    const token = document.querySelector('meta[name="x-xsrf-token"]')?.getAttribute('content');
    const headers = { 'Content-Type': 'application/json' };
    if (token) headers['RequestVerificationToken'] = token;

    const res = await fetch('/api/comments', {
      method: 'POST',
      headers,
      body: JSON.stringify(dto)
    });
    return res;
  }

  document.addEventListener('DOMContentLoaded', function(){
    const viz = document.querySelector('.visualization-placeholder');
    if(!viz) return;
    // prefer server-rendered container + form (partial). fallback to creating them dynamically if missing.
    let commentsContainer = document.getElementById('commentsContainer');
    if (!commentsContainer) {
      commentsContainer = document.createElement('div');
      commentsContainer.id = 'commentsContainer';
      commentsContainer.className = 'comments-section';
      viz.appendChild(commentsContainer);
    }

    let form = document.querySelector('.comment-form');
    if (!form) {
      form = document.createElement('form');
      form.className = 'comment-form mt-3';
      form.innerHTML = `
        <h4>Comments</h4>
        <div class="mb-2">
          <label for="commentAuthor" class="form-label">Name</label>
          <input type="text" id="commentAuthor" name="Author" class="form-control" aria-label="Your name" required maxlength="100" />
        </div>
        <div class="mb-2">
          <label for="commentContent" class="form-label">Comment</label>
          <textarea id="commentContent" name="Content" class="form-control" aria-label="Your comment" required maxlength="1000" rows="3"></textarea>
        </div>
        <div>
          <button class="btn btn-primary" type="submit">Post Comment</button>
        </div>
      `;
      viz.appendChild(form);
    }

    const dbSizeInput = document.getElementById('dbSize');
    const getVisualizationId = () => {
      return parseInt(dbSizeInput?.value || '8');
    };

    async function load(){
      const vid = getVisualizationId();
      const comments = await fetchComments(vid);
      renderComments(commentsContainer, comments);
      // update hidden visualization id in form if present
      const hid = form.querySelector('[name="VisualizationId"]');
      if (hid) hid.value = vid;
    }
    form.addEventListener('submit', async function(e){
      e.preventDefault();
      // update hidden visualization id if present
      const hid = form.querySelector('[name="VisualizationId"]');
      if (hid) hid.value = getVisualizationId();
      const authorEl = form.querySelector('[name="Author"]') || form.querySelector('#commentAuthor');
      const contentEl = form.querySelector('[name="Content"]') || form.querySelector('#commentContent');
      const author = (authorEl && authorEl.value || '').trim();
      const content = (contentEl && contentEl.value || '').trim();
      if(!author || !content) return;
      const dto = { visualizationId: getVisualizationId(), author, content };
      const res = await postComment(dto);
      if(res.ok){
        if (authorEl) authorEl.value = '';
        if (contentEl) contentEl.value = '';
        await load();
      } else {
        // handle ProblemDetails JSON first, otherwise fallback
        const ct = res.headers.get('content-type') || '';
        let userMsg = `Request failed (${res.status})`;
        try {
          if (ct.includes('application/problem+json') || ct.includes('application/json')) {
            const body = await res.json();
            // If it's a ProblemDetails object, show the "detail" if available
            if (body && body.detail) {
              userMsg = body.detail;
            } else {
              userMsg = JSON.stringify(body);
            }
            console.error('Server JSON response:', body);
          } else {
            const text = await res.text();
            if (text.trim().startsWith('<!DOCTYPE') || text.trim().startsWith('<html')) {
              // server returned an HTML error page — log full details and show friendly message
              console.error('Server returned HTML error page:', text);
              userMsg = `Server error (${res.status}). See developer console for details.`;
            } else {
              userMsg = text || userMsg;
            }
          }
        } catch (err) {
          console.error('Error parsing error response', err);
        }

        // If we detect a server-side DB save error, provide next-step help
        if (userMsg && userMsg.toLowerCase().includes('saving the entity')) {
          userMsg += ' (This often means the comments table does not exist — run the EF migrations: dotnet ef migrations add AddComments && dotnet ef database update)';
        }

        alert('Failed to post comment: ' + userMsg);
      }
    });

    // reload comments when dbSize changes (different visualization)
    dbSizeInput?.addEventListener('change', load);

    // initialize client-side unobtrusive validation if available
    try {
      if (window.jQuery && jQuery && jQuery.validator && jQuery.validator.unobtrusive) {
        jQuery(function(){ jQuery.validator.unobtrusive.parse(form); });
      }
    } catch (e) { /* ignore if validation not present */ }

    load();
  });
})();
